using BookingApi.Domain.Models;
using BookingApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Infrastructure.BackgroundServices;

public class PendingBookingProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<PendingBookingProcessor> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<PendingBookingProcessor> _logger = logger;

    private const int ProcessingDelayMs = 2_000;
    private const int PollingIntervalMs = 3_000;
    private const int ErrorRetryDelayMs = 5_000;
    private const int MaxRetries = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PendingBookingProcessor started");

        var options = new ParallelOptions
        {
            CancellationToken = stoppingToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount > 1
            ? (Environment.ProcessorCount >= 4 ? 4 : 2)
            : 1
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingBookingsAsync(options, stoppingToken);
                await Task.Delay(PollingIntervalMs, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PendingBookingProcessor");
                await Task.Delay(ErrorRetryDelayMs, stoppingToken);
            }
        }

        _logger.LogInformation("PendingBookingProcessor stopped");
    }

    private async Task ProcessPendingBookingsAsync(ParallelOptions options, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pendingBookings = await context.Bookings
            .AsNoTracking()
            .Where(b => b.Status == BookingStatus.Pending)
            .OrderBy(b => b.CreatedAt) // FIFO
            .Take(options.MaxDegreeOfParallelism)
            .Select(b => new { b.Id, b.EventId })
            .ToListAsync(ct);

        if (pendingBookings.Count == 0)
            return;

        _logger.LogInformation("Processing {Count} pending bookings", pendingBookings.Count);

        await Parallel.ForEachAsync(pendingBookings, options, async (pending, parallelCt) =>
        {
            await ProcessSingleBookingAsync(pending.Id, pending.EventId, parallelCt);
        });
    }

    private async Task ProcessSingleBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct)
    {
        _logger.LogInformation("Processing booking {BookingId}", bookingId);

        await Task.Delay(ProcessingDelayMs, ct);

        var success = await TryConfirmBookingWithRetryAsync(bookingId, eventId, ct);

        if (!success)
            _logger.LogWarning("Failed to confirm booking {BookingId} after retries", bookingId);
        else
            _logger.LogInformation("Successfully confirmed booking {BookingId}", bookingId);
    }

    private async Task<bool> TryConfirmBookingWithRetryAsync(
        Guid bookingId,
        Guid eventId,
        CancellationToken ct)
    {
        var attempt = 0;

        while (attempt < MaxRetries)
        {
            try
            {
                return await ConfirmBookingAsync(bookingId, eventId, ct);
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxRetries - 1)
            {
                attempt++;
                _logger.LogWarning(
                    "Concurrency conflict for booking {BookingId}, retry {Attempt}/{MaxRetries}",
                    bookingId, attempt, MaxRetries);

                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError(
                    "Failed to confirm booking {BookingId} due to concurrency after {MaxRetries} attempts",
                    bookingId, MaxRetries);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error confirming booking {BookingId}", bookingId);
                return false;
            }
        }

        return false;
    }

    private async Task<bool> ConfirmBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var booking = await context.Bookings
            .Include(e => e.Event)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

        if (booking == null)
        {
            _logger.LogWarning("Booking {BookingId} is not found", bookingId);
            return false;
        }
        else if (booking.Status != BookingStatus.Pending)
        {
            _logger.LogWarning("Booking {BookingId} is already processed", bookingId);
            return false;
        }

        if (booking.Event == null)
        {
            if (booking.Status != BookingStatus.Rejected)
            {
                booking.Status = BookingStatus.Rejected;
                booking.ProcessedAt = DateTime.UtcNow;

                await context.SaveChangesAsync(ct);
            }
            _logger.LogWarning("Event is not found for processing Booking {BookingId}", bookingId);
            return false;
        }

        if (booking.Event.AvailableSeats <= 0)
        {
            booking.Status = BookingStatus.Rejected;
            booking.ProcessedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(ct);

            _logger.LogWarning(
                "Booking {BookingId} rejected - no available seats for event {EventId}",
                bookingId, eventId);
            return false;
        }

        booking.Event.AvailableSeats--;
        booking.Status = BookingStatus.Confirmed;
        booking.ProcessedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        return true;
    }
}
