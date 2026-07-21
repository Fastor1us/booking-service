namespace BookingApi.IntegrationTests.Fixtures;

public class TestCollectionCleanup : IDisposable
{
    public void Dispose()
    {
        DatabaseContainer.Cleanup();
    }
}
