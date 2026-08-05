using BookingApi.Infrastructure.Persistence;
using BookingApi.Presentation.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApi.Presentation.Tests.Base;

[Collection("Postgre")]
public abstract class PostgreSqlBase : IAsyncLifetime
{
    private AppDbContext? _context;
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;
    private static bool _schemaCreated = false;
    private static readonly Lock _lock = new();

    public HttpClient Client => _client ?? throw new InvalidOperationException("Client not initialized");
    public WebApplicationFactory<Program> Factory => _factory ?? throw new InvalidOperationException("Factory not initialized");

    public virtual async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(GetTestConnectionString()));
                });
            });

        _client = _factory.CreateClient();

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

        if (_client != null)
        {
            _client.Dispose();
            _client = null;
        }

        if (_factory != null)
        {
            await _factory.DisposeAsync();
            _factory = null;
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

    protected static string GetTestConnectionString()
    {
        var container = DatabaseContainer.GetContainer();
        return container.GetConnectionString();
    }
}
