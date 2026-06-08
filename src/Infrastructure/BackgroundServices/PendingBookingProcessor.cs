using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;

namespace BookingApi.Infrastructure.BackgroundServices;

public class PendingBookingProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<PendingBookingProcessor> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<PendingBookingProcessor> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PendingBookingProcessor has been started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider
                    .GetRequiredService<IBookingRepository>();

                var result = await bookingRepository
                    .TryGetPendingBooking(stoppingToken);

                while (result.Success && result.Booking != null)
                {
                    var booking = result.Booking;

                    if (booking.Status == BookingStatus.Pending)
                    {
                        await Task.Delay(2_000, stoppingToken);

                        var confirmResult = await bookingRepository
                            .TryConfirmBooking(booking.Id, stoppingToken);

                        if (!confirmResult.Success)
                            _logger.LogWarning(
                                "Failed to confirm booking {BookingId}: {Details}",
                                booking.Id, confirmResult.Details);
                        else if (_logger.IsEnabled(LogLevel.Information))
                            _logger.LogInformation(
                                "Successfully confirmed pending booking {BookingId}",
                                booking.Id);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"TryGetPendingBooking returned booking with invalid status. " +
                            $"Booking id: {booking.Id}, status: {booking.Status}");
                    }

                    result = await bookingRepository
                        .TryGetPendingBooking(stoppingToken);
                }

                await Task.Delay(3_000, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PendingBookingProcessor");
                await Task.Delay(5_000, stoppingToken);
            }
        }

        _logger.LogInformation("PendingBookingProcessor has been stopped");
    }
}
