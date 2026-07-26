namespace BookingApi.Presentation.Domain.Models;

public readonly struct BookingRepositoryResult
{
    public bool IsSuccess { get; }
    public Booking? Booking { get; }
    public BookingErrorType ErrorType { get; }
    public string? ErrorMessage { get; }

    public BookingRepositoryResult(bool success, Booking booking)
    {
        IsSuccess = success;
        Booking = booking;
        ErrorType = BookingErrorType.None;
        ErrorMessage = null;
    }

    public BookingRepositoryResult(
        bool isSuccess,
        BookingErrorType errorType,
        string errorMessage,
        Booking? booking = null)
    {
        IsSuccess = isSuccess;
        Booking = booking;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public static BookingRepositoryResult Success(Booking booking)
        => new(true, booking);

    public static BookingRepositoryResult BookingNotFound(Guid bookingId)
        => new(
            isSuccess: false,
            errorType: BookingErrorType.BookingNotFound,
            errorMessage: $"Booking with id '{bookingId}' is not exist");

    public static BookingRepositoryResult EventNotFound(
        Guid eventId, Booking? booking = null)
        => new(
            isSuccess: false,
            errorType: BookingErrorType.EventNotFound,
            errorMessage: $"Event with id '{eventId}' is not exist",
            booking);

    public static BookingRepositoryResult NoAvailableSeats()
        => new(
            isSuccess: false,
            errorType: BookingErrorType.NoAvailableSeats,
            errorMessage: "No available seats for this event");

    public static BookingRepositoryResult BookingAlreadyExists(Guid id)
        => new(
            isSuccess: false,
            errorType: BookingErrorType.BookingAlreadyExists,
            errorMessage: $"Booking with id '{id}' already exists");

    public static BookingRepositoryResult InvalidStatus(
        Booking booking, BookingStatus status)
        => new(
            isSuccess: false,
            errorType: BookingErrorType.InvalidStatus,
            errorMessage: $"Booking with id '{booking.Id}' is not in {status} status",
            booking);
}

public enum BookingErrorType
{
    None,
    EventNotFound,
    NoAvailableSeats,
    BookingAlreadyExists,
    BookingNotFound,
    InvalidStatus
}
