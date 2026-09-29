using BookingService.Application.Interfaces;
using BookingService.Domain.Models;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BookingService.Infrastructure.BackgroundServices;

public sealed class BookingMissingAnswerResolver(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingMissingAnswerResolver> logger) : BackgroundService
{
    public TimeSpan ScanInterval { private get; init; } = TimeSpan.FromMinutes(1);
    public TimeSpan StuckThreshold { private get; init; } = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation(
            "BookingMissingAnswerResolver started. ScanInterval={ScanInterval}, StuckThreshold={StuckThreshold}",
            ScanInterval, StuckThreshold);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await SweepAsync(ct);
                await Task.Delay(ScanInterval, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unhandled error while sweeping stuck bookings. Will retry after {ScanInterval}",
                    ScanInterval);
            }
        }

        logger.LogInformation("BookingMissingAnswerResolver stopped.");
    }

    private async Task SweepAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var compensator = scope.ServiceProvider.GetRequiredService<IOutboxCompensator>();

        var threshold = DateTimeOffset.UtcNow - StuckThreshold;

        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var stuck = await unitOfWork.Bookings.ToListAsync(
                unitOfWork.Bookings.GetQuery()
                    .Where(e =>
                        (e.Status == BookingStatus.Pending && e.ProcessedAt < threshold)
                        || (e.Status == BookingStatus.Cancelling && e.ProcessedAt < threshold)),
                ct);

            if (stuck.Count == 0)
            {
                await unitOfWork.CommitTransactionAsync(ct);
                return;
            }

            logger.LogWarning(
                "Found {Count} stuck bookings older than {Threshold}. BookingIds={BookingIds}",
                stuck.Count, threshold, stuck.Select(b => b.Id));

            foreach (var booking in stuck)
            {
                await CompensateBookingAsync(booking, compensator, ct);
            }

            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);

            logger.LogInformation(
                "Sweep completed. ProcessedBookings={Count}", stuck.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Sweep failed, transaction will be rolled back. StuckThreshold={Threshold}",
                StuckThreshold);

            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task CompensateBookingAsync(
        Booking booking,
        IOutboxCompensator compensator,
        CancellationToken ct)
    {
        var messageType = booking.Status == BookingStatus.Pending
            ? Commands.ReserveSeat
            : Commands.ReleaseSeat;

        object payload = booking.Status == BookingStatus.Pending
            ? new ReserveEventSeat(booking.Id, booking.EventId)
            : new ReleaseEventSeat(booking.Id, booking.EventId);

        var correlationId = Guid.NewGuid();

        try
        {
            var compensated = await compensator.TryCompensateAsync(new OutboxMessage
            {
                Id = correlationId,
                CorrelationId = correlationId,
                Key = booking.EventId.ToString(),
                Topic = string.Empty,
                MessageType = messageType,
                Payload = JsonSerializer.Serialize(payload)
            }, ct);

            if (compensated)
            {
                logger.LogInformation(
                    "Compensation applied for stuck booking. BookingId={BookingId}, EventId={EventId}, Status={Status}, MessageType={MessageType}, CorrelationId={CorrelationId}",
                    booking.Id, booking.EventId, booking.Status, messageType, correlationId);
            }
            else
            {
                logger.LogWarning(
                    "Compensation skipped (no-op) for stuck booking. BookingId={BookingId}, Status={Status}, MessageType={MessageType}, CorrelationId={CorrelationId}",
                    booking.Id, booking.Status, messageType, correlationId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Compensation failed for stuck booking. BookingId={BookingId}, Status={Status}, MessageType={MessageType}, CorrelationId={CorrelationId}",
                booking.Id, booking.Status, messageType, correlationId);
        }
    }
}
