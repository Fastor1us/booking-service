using BookingService.Domain.Models;

namespace BookingService.Presentation.Dtos;

public class BookingResponseDto
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required BookingStatus Status { get; set; }
}
