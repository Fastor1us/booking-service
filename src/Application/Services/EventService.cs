using BookingApi.Application.Interfaces;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;
using BookingApi.Infrastructure.Data;
using BookingApi.Presentation.Dtos;
using BookingApi.Presentation.Filters;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Application.Services;

public class EventService(AppDbContext context) : IEventService
{
    public async Task<Event> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new EventNotFoundException(id);
    }

    public async Task<PagedEvents> GetAllAsync(
        EventFilter filter,
        PaginationParams paginationParams,
        CancellationToken ct)
    {
        using var transaction = await context.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Snapshot, ct);

        var query = context.Events
            .AsNoTrackingWithIdentityResolution();

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

        await transaction.CommitAsync(ct);

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

        context.Events.Add(@event);
        await context.SaveChangesAsync(ct);

        return @event;
    }

    public async Task UpdateAsync(Guid id, UpdateEventDto dto, CancellationToken ct)
    {
        await context.Events
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(e => e.Title, _ => dto.Title)
                    .SetProperty(e => e.Description, _ => dto.Description)
                    .SetProperty(e => e.StartAt, _ => dto.StartAt)
                    .SetProperty(e => e.EndAt, _ => dto.EndAt),
                ct);
    }

    public async Task RemoveAsync(Guid id, CancellationToken ct)
    {
        await context.Events
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync(ct);
    }
}
