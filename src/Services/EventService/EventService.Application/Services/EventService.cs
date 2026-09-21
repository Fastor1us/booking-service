using EventService.Application.Dtos;
using EventService.Application.Interfaces;
using EventService.Application.Options;
using EventService.Domain.Constants;
using EventService.Domain.Exceptions;
using EventService.Domain.Models;
using Messaging.Abstractions.Persistence;
using Microsoft.Extensions.Options;

namespace EventService.Application.Services;

public class EventService(
    IUnitOfWork unitOfWork,
    IEventCache cache,
    IOptions<EventCacheOptions> cacheOptions) : IEventService
{
    public Task<List<Event>> GetTopAsync(CancellationToken ct)
    {
        return cache.GetOrSetAsync(
            key: "events:top10",
            factory: async () => await unitOfWork.Events.ToListAsync(
                unitOfWork.Events
                    .GetQuery()
                    .OrderBy(e => (e.TotalSeats - e.AvailableSeats) / e.TotalSeats)
                    .Take(10)),
            ttl: cacheOptions.Value.TopEventsTtl,
            ct: ct)!;
    }

    public Task<Event> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return cache.GetOrSetAsync(
            key: $"event:{id}",
            factory: async () => await unitOfWork.Events
                    .FirstOrDefaultAsync(
                        QueryTrackerBehavior.NoTracking,
                        e => e.Id == id,
                        ct) 
                ?? throw new EventNotFoundException(id),
            ttl: cacheOptions.Value.EventTtl,
            ct: ct)!;
    }

    public async Task<PagedEventsDto> GetAllAsync(
        EventFilterDto filter,
        PaginationParamsDto paginationParams,
        CancellationToken ct)
    {
        await unitOfWork.BeginTransactionAsync(
            System.Data.IsolationLevel.RepeatableRead, ct);

        var eventRepository = unitOfWork.Events;

        var query = eventRepository
            .GetQuery(QueryTrackerBehavior.NoTrackingWithIdentityResolution);

        if (!string.IsNullOrWhiteSpace(filter.Title))
            query = query.Where(e => e.Title.Contains(filter.Title));

        if (filter.From.HasValue)
            query = query.Where(e => e.StartAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(e => e.StartAt <= filter.To.Value);

        query = query.OrderByDescending(e => e.StartAt);

        var totalCount = await eventRepository.CountAsync(query, ct);

        query = query
                    .Skip((paginationParams.PageIndex - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize);

        var items = await eventRepository.ToListAsync(query, ct);

        await unitOfWork.CommitTransactionAsync(ct);

        return new PagedEventsDto(items, totalCount);
    }

    public async Task<Event> AddAsync(CreateEventDto dto, CancellationToken ct)
    {
        var @event = new Event
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            TotalSeats = dto.TotalSeats,
            AvailableSeats = dto.TotalSeats,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt
        };

        unitOfWork.Events.Add(@event);
        await unitOfWork.SaveChangesAsync(ct);
        await cache.SetAsync($"event:{@event.Id}", @event, cacheOptions.Value.EventTtl);

        return @event;
    }

    public async Task UpdateAsync(Guid id, UpdateEventDto dto, CancellationToken ct)
    {
        var @event = new Event
        {
            Id = id,
            Title = dto.Title,
            Description = dto.Description,
            TotalSeats = EventConstants.MinTotalSeats, // required
            StartAt = dto.StartAt,
            EndAt = dto.EndAt
        };

        var isRemoved = await unitOfWork.Events
            .ExecuteUpdateByIdAsync(@event, ct) == 1;

        if (isRemoved)
        {
            await cache.RemoveAsync($"event:{@event.Id}", ct);
        }
    }

    public async Task RemoveAsync(Guid id, CancellationToken ct)
    {
        var isRemoved = await unitOfWork.Events
            .ExecuteDeleteByIdAsync(id, ct) == 1;

        if (isRemoved)
        {
            await cache.RemoveAsync($"event:{id}", ct);
        }
        else
        {
            throw new EventNotFoundException(id);
        }
    }
}
