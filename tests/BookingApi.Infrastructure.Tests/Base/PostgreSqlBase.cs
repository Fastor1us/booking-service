using BookingApi.Infrastructure.Persistence;
using BookingApi.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Infrastructure.Tests.Base;

[Collection("Postgre")]
public abstract class PostgreSqlBase : IAsyncLifetime
{
    private AppDbContext? _context;
    private static bool _schemaCreated = false;
    private static readonly Lock _lock = new();

    public async Task InitializeAsync()
    {
        _context = CreateContext();

        if (!_schemaCreated)
        {
            lock (_lock)
            {
                if (!_schemaCreated)
                {
                    _context.Database.Migrate();
                    _schemaCreated = true;
                }
            }
        }

        await TruncateTables();
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
            _context = null;
        }
    }

    private async Task TruncateTables()
    {
        var tableNames = _context!.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToList();

        foreach (var tableName in tableNames!)
        {
            await _context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{tableName}\" CASCADE;");
        }
    }

    protected AppDbContext CreateContext()
    {
        var container = DatabaseContainer.GetContainer();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(container.GetConnectionString())
            .Options;
        return new AppDbContext(options);
    }
}
