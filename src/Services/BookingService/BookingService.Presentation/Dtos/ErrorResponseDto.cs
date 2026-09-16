namespace BookingService.Presentation.Dtos;

public class ErrorResponseDto
{
    public required string Title { get; init; }
    public IEnumerable<string> Details { get; init; } = [];
}
