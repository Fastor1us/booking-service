using BookingApi.Domain.Models;

namespace BookingApi.Presentation.Dtos;

public class BookingResponseDto
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required BookingStatus Status { get; set; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; set; }
}
