using BookingApi.Infrastructure.Data;
using BookingApi.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.IntegrationTests.Base;

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

        await RecreateSchemaViaSqlAsync();
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
            _context = null;
        }
    }

    private async Task RecreateSchemaViaSqlAsync()
    {
        var createScript = _context!.Database.GenerateCreateScript();

        var commands = createScript.Split(';', StringSplitOptions.RemoveEmptyEntries);

        //await _context.Database.ExecuteSqlRawAsync("SET session_replication_role = replica;");

        var tableNames = _context.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToList();

        foreach (var tableName in tableNames!)
        {
            await _context.Database.ExecuteSqlRawAsync($"DROP TABLE IF EXISTS \"{tableName}\" CASCADE;");
        }

        foreach (var command in commands)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                await _context.Database.ExecuteSqlRawAsync(command + ";");
            }
        }

        //await _context.Database.ExecuteSqlRawAsync("SET session_replication_role = DEFAULT;");
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
