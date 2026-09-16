using BookingService.Application.Interfaces;
using BookingService.Domain.Models;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System.Text.Json;

namespace BookingService.Infrastructure.BackgroundServices;

public sealed class BookingMissingAnswerResolver(
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

    public TimeSpan ScanInterval { private get; init; } = TimeSpan.FromMinutes(1);
    public TimeSpan StuckThreshold { private get; init; } = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
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
                _logger.Error($"Error while sweeping Bookings: {ex}");
            }
        }
    }

    private async Task SweepAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();
        var compensator = scope.ServiceProvider
            .GetRequiredService<IOutboxCompensator>();

        await unitOfWork.BeginTransactionAsync(ct);

        var threshold = DateTimeOffset.UtcNow - StuckThreshold;

        var stuck = await unitOfWork.Bookings.ToListAsync(
            unitOfWork.Bookings.GetQuery()
                .Where(e =>
                    (e.Status == BookingStatus.Pending && e.ProcessedAt < threshold)
                    || (e.Status == BookingStatus.Cancelling && e.ProcessedAt < threshold)
                    ), ct);

        if (stuck.Count == 0)
            return;

        foreach (var booking in stuck)
        {
            try
            {
                string messageType = booking.Status == BookingStatus.Pending
                        ? Commands.ReserveSeat : Commands.ReleaseSeat;
                object payload = booking.Status == BookingStatus.Pending
                        ? new ReserveEventSeat(booking.Id, booking.EventId)
                        : new ReleaseEventSeat(booking.Id, booking.EventId);

                await compensator.TryCompensateAsync(new OutboxMessage()
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid(),
                    Key = string.Empty,
                    Topic = string.Empty,
                    MessageType = messageType,
                    Payload = JsonSerializer.Serialize(payload)
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Compensation failed for booking {BookingId}", booking.Id);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        await unitOfWork.CommitTransactionAsync(ct);
    }
}
