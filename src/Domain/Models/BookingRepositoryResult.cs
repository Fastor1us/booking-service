namespace BookingApi.Domain.Models;

public readonly struct BookingRepositoryResult
{
    public bool Success { get; }
    public Booking? Booking { get; }
    public string? Details { get; }

    public BookingRepositoryResult(bool success, Booking booking)
    {
        Success = success;
        Booking = booking;
        Details = null;
    }

    public BookingRepositoryResult(
        bool success, string details, Booking? booking = null)
    {
        Success = success;
        Booking = booking;
        Details = details;
    }
}
