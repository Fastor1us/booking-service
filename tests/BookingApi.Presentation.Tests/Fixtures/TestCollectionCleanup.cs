namespace BookingApi.Presentation.Tests.Fixtures;

public class TestCollectionCleanup : IDisposable
{
    public void Dispose()
    {
        DatabaseContainer.Cleanup();
    }
}
