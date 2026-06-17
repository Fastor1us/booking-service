using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;

namespace BookingApi.Infrastructure.BackgroundServices;

public class PendingBookingProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<PendingBookingProcessor> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<PendingBookingProcessor> _logger = logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PendingBookingProcessor has been started");

        ParallelOptions _parallelOptions = new()
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
                using var scope = _scopeFactory.CreateScope();
                var bookingRepository = scope.ServiceProvider
                    .GetRequiredService<IBookingRepository>();

                var pendingBookingIds = await bookingRepository
                    .TryGetPendingBookingIds(stoppingToken);

                await Parallel.ForEachAsync(
                    pendingBookingIds,
                    _parallelOptions,
                    async (id, linkedToken) =>
                    {
                        _logger.LogInfo($"Start handle Booking with id: {id}");

                        await Task.Delay(2_000, linkedToken);

                        BookingRepositoryResult confirmResult;
                        await _semaphore.WaitAsync(linkedToken);
                        try
                        {
                            confirmResult = await bookingRepository
                                .TryConfirmBooking(id, linkedToken);
                        }
                        finally
                        {
                            _semaphore.Release();
                        }

                        if (!confirmResult.IsSuccess)
                            _logger.LogWarning(
                                "Failed to confirm booking {BookingId}: {Details}",
                                id, confirmResult.ErrorMessage);
                        else
                            _logger.LogInfo(
                                "Successfully confirmed pending booking {BookingId}",
                                id);
                    });

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
