using BookingApi.Application.Interfaces;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;
using BookingApi.Presentation.Dtos;
using BookingApi.Presentation.Filters;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Application.Services;

public class EventService(IUnitOfWork unitOfWork) : IEventService
{
    public async Task<Event> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await unitOfWork.EventRepository
            .GetQuery(QueryTrackerBehavior.NoTracking)
            .FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new EventNotFoundException(id);
    }

    public async Task<PagedEvents> GetAllAsync(
        EventFilter filter,
        PaginationParams paginationParams,
        CancellationToken ct)
    {
        await unitOfWork.BeginTransactionAsync(
            System.Data.IsolationLevel.RepeatableRead, ct);

        var query = unitOfWork.EventRepository
            .GetQuery(QueryTrackerBehavior.NoTrackingWithIdentityResolution);

        if (!string.IsNullOrWhiteSpace(filter.Title))
            query = query.Where(e => e.Title.Contains(filter.Title));

        if (filter.From.HasValue)
            query = query.Where(e => e.StartAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(e => e.StartAt <= filter.To.Value);

        query = query.OrderByDescending(e => e.StartAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((paginationParams.PageIndex - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(ct);

        await unitOfWork.CommitTransactionAsync(ct);

        return new PagedEvents(items, totalCount);
    }

    public async Task<Event> AddAsync(CreateEventDto dto, CancellationToken ct)
    {
        var @event = new Event
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            TotalSeats = dto.TotalSeats,
            AvailableSeats = dto.TotalSeats,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt
        };

        unitOfWork.EventRepository.Add(@event);
        await unitOfWork.SaveChangesAsync(ct);

        return @event;
    }

    public async Task UpdateAsync(Guid id, UpdateEventDto dto, CancellationToken ct)
    {
        await unitOfWork.EventRepository
            .ExecuteUpdateByIdAsync(id, dto.Title, dto.Description, dto.StartAt, dto.EndAt, ct);
    }

    public async Task RemoveAsync(Guid id, CancellationToken ct)
    {
        await unitOfWork.EventRepository
            .ExecuteDeleteByIdAsync(id, ct);
    }
}
