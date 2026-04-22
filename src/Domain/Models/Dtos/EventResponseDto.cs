using System.ComponentModel.DataAnnotations;

namespace BookingApi.Domain.Models.Dtos;

public class EventResponseDto
{
    [Required]
    public required Guid Id { get; set; }
    [Required]
    public required string Title { get; set; }
    public string? Description { get; set; }
    [Required]
    public required DateTime StartAt { get; set; }
    [Required]
    public required DateTime EndAt { get; set; }
}
