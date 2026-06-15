using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
using BookingApi.Presentation.Dtos;
using System.Collections.Concurrent;

namespace BookingApi.Infrastructure.Repositories;

public class EventInMemoryRepository : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _events = new();

    public EventInMemoryRepository()
    {
        // [x] Use to generate base events! UwU
        // UNCOMMENT THIS BLOCK TO POPULATE THE REPOSITORY WITH TEST DATA
        // (20 events with sequential dates)

        // int length = 20;

        // var events = Enumerable.Range(1, length).Select((index) =>
        // {
        //     Guid guid = Guid.NewGuid();
        //     DateTime date = DateTime.Now;
        //     return new Event
        //     {
        //         Id = guid,
        //         Title = "Title #" + index.ToString(),
        //         StartAt = date.AddDays(-1 * (length - index)),
        //         EndAt = date.AddDays(-1 * (length - index - 1))
        //     };
        // });
        // foreach (var @event in events)
        // {
        //     _events.TryAdd(@event.Id, @event);
        // }
    }

    public Task<Event?> TryGetByIdAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _events.TryGetValue(id, out var @event);

        return Task.FromResult(@event);
    }

    public Task<PagedEvents> GetPagedAsync(
        IQueryable<Event> query,
        int pageIndex,
        int pageSize,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var totalCount = query.Count();

        var items = query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedEvents(items, totalCount));
    }

    public Task<IQueryable<Event>> GetQueryableAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(_events.Values.AsQueryable());
    }

    public Task<Guid> AddAsync(CreateEventDto @event, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var id = Guid.NewGuid();

        if (!_events.TryAdd(id, new Event
        {
            Id = id,
            Title = @event.Title,
            Description = @event.Description,
            TotalSeats = @event.TotalSeats,
            AvailableSeats = @event.TotalSeats,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt
        }))
        {
            return AddAsync(@event, ct);
        }

        return Task.FromResult(id);
    }

    public Task<bool> TryUpdateAsync(
        Guid id, UpdateEventDto updateEvent, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_events.TryGetValue(id, out Event? existedEvent))
        {
            return Task.FromResult(false);
        }

        var newEvent = new Event
        {
            Id = existedEvent.Id,
            Title = updateEvent.Title,
            Description = updateEvent.Description,
            TotalSeats = existedEvent.TotalSeats,
            AvailableSeats = existedEvent.AvailableSeats,
            StartAt = updateEvent.StartAt,
            EndAt = updateEvent.EndAt
        };

        var res = _events.TryUpdate(id, newEvent, existedEvent);
        return Task.FromResult(res);
    }

    public Task<bool> TryRemoveAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var res = _events.TryRemove(id, out _);
        return Task.FromResult(res);
    }
}
