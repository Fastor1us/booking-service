using BookingApi.Application.Dtos;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Tests.Helpers;

public static class EventFactory
{
    public static Event Generate(
        Guid? guid = null,
        string? title = null,
        DateTime? startAt = null,
        DateTime? endAt = null,
        int? totalSeats = null,
        int? availableSeats = null)
    {
        return new Event
        {
            Id = guid ?? Guid.NewGuid(),
            Title = title ?? "Event title",
            TotalSeats = totalSeats ?? 20,
            AvailableSeats = availableSeats ?? totalSeats ?? 20,
            StartAt = startAt ?? DateTime.Now.AddDays(1),
            EndAt = endAt ?? DateTime.Now.AddDays(2)
        };
    }

    public static List<Event> Generate(int count)
    {
        return [.. Enumerable
            .Range(1, count)
            .Select((index) =>
            {
                DateTime date = DateTime.Now;
                return new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Title #" + index.ToString(),
                    TotalSeats = 20,
                    AvailableSeats = 20,
                    StartAt = date.AddDays(1 + index),
                    EndAt = date.AddDays(2  + index)
                };
        })];
    }

    public static T Generate<T>(
        string? title = null,
        string? description = null,
        DateTime? startAt = null,
        DateTime? endAt = null,
        int? totalSeats = null) where T : class
    {
        return typeof(T).Name switch
        {
            nameof(CreateEventDto) => new CreateEventDto
            {
                Title = title ?? "Event title",
                Description = description,
                TotalSeats = totalSeats ?? 20,
                StartAt = startAt ?? DateTime.Now.AddDays(1),
                EndAt = endAt ?? DateTime.Now.AddDays(2)
            } as T ?? throw new InvalidCastException(),

            nameof(UpdateEventDto) => new UpdateEventDto
            {
                Title = title ?? "Updated Event",
                StartAt = startAt ?? DateTime.Now.AddDays(1),
                EndAt = endAt ?? DateTime.Now.AddDays(2)
            } as T ?? throw new InvalidCastException(),

            nameof(Event) => new Event
            {
                Id = Guid.NewGuid(),
                Title = title ?? "Event title",
                Description = description,
                TotalSeats = totalSeats ?? 20,
                AvailableSeats = totalSeats ?? 20,
                StartAt = startAt ?? DateTime.Now.AddDays(1),
                EndAt = endAt ?? DateTime.Now.AddDays(2)
            } as T ?? throw new InvalidCastException(),

            _ => throw new NotSupportedException(
                $"Type {typeof(T).Name} is not supported"),
        };
    }
}
