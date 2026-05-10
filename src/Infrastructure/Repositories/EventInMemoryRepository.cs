using BookingApi.Application.Interfaces;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;
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

    public async Task<Event> GetById(Guid id)
    {
        if (_events.TryGetValue(id, out var @event))
            return await Task.FromResult(@event);

        throw new EventNotFoundException(id);
    }

    public async Task<PagedEvents> GetPaged(
        IQueryable<Event> query,
        int pageIndex,
        int pageSize)
    {
        var totalCount = query.Count();

        var items = query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return await Task.FromResult(new PagedEvents(items, totalCount));
    }

    public async Task<IQueryable<Event>> GetQueryable()
    {
        return await Task.FromResult(_events.Values.AsQueryable());
    }

    public async Task<Guid> Add(Event @event)
    {
        var newId = Guid.NewGuid();

        var newEvent = new Event
        {
            Id = newId,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt
        };

        if (!_events.TryAdd(newId, newEvent))
            throw new InvalidOperationException($"Failed to add event with id {newId}");

        return await Task.FromResult(newId);
    }

    public async Task Update(Event @event)
    {
        if (!_events.TryUpdate(@event.Id, @event, _events[@event.Id]))
            throw new EventNotFoundException(@event.Id);

        await Task.CompletedTask;
    }

    public async Task Remove(Guid id)
    {
        if (!_events.TryRemove(id, out _))
            throw new EventNotFoundException(id);

        await Task.CompletedTask;
    }
}
