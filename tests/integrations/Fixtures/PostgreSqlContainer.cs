using Testcontainers.PostgreSql;

namespace BookingApi.IntegrationTests.Fixtures;

public static class DatabaseContainer
{
    private static PostgreSqlContainer? _container;
    private static readonly Lock _lock = new();
    private static bool _isStarted;

    public static PostgreSqlContainer GetContainer()
    {
        if (_container == null)
        {
            lock (_lock)
            {
                if (_container == null)
                {
                    Console.WriteLine("🚀 Creating and starting PostgreSQL container...");
                    _container = new PostgreSqlBuilder("postgres:16-alpine")
                        .Build();

                    _container.StartAsync().GetAwaiter().GetResult();
                    _isStarted = true;
                    Console.WriteLine($"✅ PostgreSQL container started at {_container.GetConnectionString()}");
                }
            }
        }
        return _container;
    }

    public static void Cleanup()
    {
        if (_isStarted && _container != null)
        {
            Console.WriteLine("🧹 Cleaning up PostgreSQL container...");
            try
            {
                _container.DisposeAsync().GetAwaiter().GetResult();
                _isStarted = false;
                Console.WriteLine("✅ Container disposed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during container cleanup: {ex.Message}");
            }
        }
    }
}
