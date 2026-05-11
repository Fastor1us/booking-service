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

                while (await bookingRepository
                    .TryGetPendingBooking(out Booking? booking, stoppingToken))
                {
                    if (booking != null &&
                        booking.Status == BookingStatus.Pending)
                    {
                        await Task.Delay(2_000, stoppingToken);

                        await bookingRepository
                            .ConfirmBooking(booking.Id, stoppingToken);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "TryGetPendingBooking returned true but booking" +
                            "is null or has invalid status. Booking id: " +
                            $"{booking?.Id}, status: {booking?.Status}");
                    }
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
