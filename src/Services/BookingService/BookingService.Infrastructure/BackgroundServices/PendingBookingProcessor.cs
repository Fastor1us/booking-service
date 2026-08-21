using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.BackgroundServices;

public class PendingBookingProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<PendingBookingProcessor> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<PendingBookingProcessor> _logger = logger;

    private const int ProcessingDelayMs = 2_000;
    private const int PollingIntervalMs = 3_000;
    private const int ErrorRetryDelayMs = 5_000;

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
        //using var scope = _scopeFactory.CreateScope();
        //var data = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        //var pendingBookings = await data.BookingRepository
        //    .ToListAsync(data.BookingRepository
        //        .GetQuery(QueryTrackerBehavior.NoTracking)
        //        .Where(b => b.Status == BookingStatus.Pending)
        //        .OrderBy(b => b.CreatedAt) // FIFO
        //        .Take(options.MaxDegreeOfParallelism)
        //        , ct);

        //if (pendingBookings.Count == 0)
        //    return;

        //_logger.LogInformation("Processing {Count} pending bookings", pendingBookings.Count);

        //await Parallel.ForEachAsync(pendingBookings, options, async (pending, parallelCt) =>
        //{
        //    await ProcessSingleBookingAsync(pending.Id, pending.EventId, parallelCt);
        //});
    }

    //private async Task ProcessSingleBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct)
    //{
    //    _logger.LogInformation("Processing booking {BookingId}", bookingId);

    //    await Task.Delay(ProcessingDelayMs, ct);

    //    var success = await TryConfirmBookingWithRetryAsync(bookingId, eventId, ct);

    //    if (!success)
    //        _logger.LogWarning("Failed to confirm booking {BookingId} after retries", bookingId);
    //    else
    //        _logger.LogInformation("Successfully confirmed booking {BookingId}", bookingId);
    //}

    //private async Task<bool> TryConfirmBookingWithRetryAsync(
    //    Guid bookingId,
    //    Guid eventId,
    //    CancellationToken ct)
    //{
    //    using var scope = _scopeFactory.CreateScope();
    //    var data = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    //    return await data.ExecuteWithRetryAsync(_ =>
    //        ConfirmBookingAsync(bookingId, eventId, ct),
    //        ct);
    //}

    //private async Task<bool> ConfirmBookingAsync(Guid bookingId, Guid eventId, CancellationToken ct)
    //{
    //    using var scope = _scopeFactory.CreateScope();
    //    var data = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    //    var booking = await data.BookingRepository.FirstOrDefaultAsync(b => b.Id == bookingId, ct);

    //    if (booking == null)
    //    {
    //        _logger.LogWarning("Booking {BookingId} is not found", bookingId);
    //        return false;
    //    }
    //    else if (booking.Status != BookingStatus.Pending)
    //    {
    //        _logger.LogWarning("Booking {BookingId} is already processed", bookingId);
    //        return false;
    //    }

    //    if (booking.Event == null)
    //    {
    //        if (booking.Status != BookingStatus.Rejected)
    //        {
    //            booking.Status = BookingStatus.Rejected;
    //            booking.ProcessedAt = DateTime.UtcNow;

    //            await data.SaveChangesAsync(ct);
    //        }
    //        _logger.LogWarning("Event is not found for processing Booking {BookingId}", bookingId);
    //        return false;
    //    }

    //    if (booking.Event.AvailableSeats <= 0)
    //    {
    //        booking.Status = BookingStatus.Rejected;
    //        booking.ProcessedAt = DateTime.UtcNow;

    //        await data.SaveChangesAsync(ct);

    //        _logger.LogWarning(
    //            "Booking {BookingId} rejected - no available seats for event {EventId}",
    //            bookingId, eventId);
    //        return false;
    //    }

    //    booking.Status = BookingStatus.Confirmed;
    //    booking.ProcessedAt = DateTime.UtcNow;

    //    await data.SaveChangesAsync(ct);

    //    return true;
    //}
}
