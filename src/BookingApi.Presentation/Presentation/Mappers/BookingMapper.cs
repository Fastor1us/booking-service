using BookingApi.Presentation.Domain.Models;
using BookingApi.Presentation.Presentation.Dtos;

namespace BookingApi.Presentation.Presentation.Mappers;

public static class BookingMapper
{
    public static BookingResponseDto MapToResponseDto(this Booking booking)
    {
        return new BookingResponseDto()
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt,
        };
    }
}
