using Messaging.Abstractions.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Messaging.Inbox;

public class InboxSweeper(IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

    public TimeSpan ScanInterval { private get; init; } = TimeSpan.FromMinutes(30);
    public TimeSpan OutdateThreshold { private get; init; } = TimeSpan.FromDays(30);

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
                _logger.Error($"Error while sweeping Inbox: {ex}");
            }
        }
    }

    private async Task SweepAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWorkBase>();

        var threshold = DateTimeOffset.UtcNow - OutdateThreshold;

        var deleted = await unitOfWork.InboxMessages.
            ExecuteDeleteOutdatedAsync(threshold, ct);

         _logger.Info($"Sweeped {deleted} rows from Inbound table");
    }
}
