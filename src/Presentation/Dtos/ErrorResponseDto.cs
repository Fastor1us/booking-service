namespace BookingApi.Presentation.Dtos;

public class ErrorResponseDto
{
    public required string Title { get; init; }
    public List<string> Details { get; init; } = [];
}
