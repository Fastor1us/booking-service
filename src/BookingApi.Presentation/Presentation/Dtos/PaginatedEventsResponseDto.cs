namespace BookingApi.Presentation.Presentation.Dtos;

public class PaginatedEventsResponseDto
{
    public IEnumerable<EventResponseDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageIndex { get; init; }
    public int ItemsCount { get; set; }
}
