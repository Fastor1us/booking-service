namespace BookingApi.Infrastructure.Tests.Fixtures;

public class TestCollectionCleanup : IDisposable
{
    public void Dispose()
    {
        DatabaseContainer.Cleanup();
    }
}
