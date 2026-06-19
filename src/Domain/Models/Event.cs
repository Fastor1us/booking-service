namespace BookingApi.Domain.Models;

public class Event
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }

    private readonly Lock _locker = new();

    public bool TryReserveSeats(int count = 1)
    {
        if (count < 1) return false;

        lock (_locker)
        {
            if (AvailableSeats - count >= 0)
            {
                AvailableSeats -= count;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool TryReleaseSeats(int count = 1)
    {
        if (count < 1) return false;

        lock (_locker)
        {
            if (AvailableSeats + count <= TotalSeats)
            {
                AvailableSeats += count;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
