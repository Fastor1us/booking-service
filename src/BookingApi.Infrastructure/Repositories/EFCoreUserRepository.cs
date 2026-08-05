using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
using BookingApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Infrastructure.Repositories;

public sealed class EFCoreUserRepository(AppDbContext context)
    : EFCoreRepositoryBase<User>, IUserRepository
{
    public override IQueryable<User> GetQuery(
        QueryTrackerBehavior behavior = QueryTrackerBehavior.Track)
    {
        return behavior switch
        {
            QueryTrackerBehavior.Track =>
                context.Users,
            QueryTrackerBehavior.NoTracking =>
                context.Users.AsNoTracking(),
            QueryTrackerBehavior.NoTrackingWithIdentityResolution =>
                context.Users.AsNoTrackingWithIdentityResolution(),
            _ =>
                context.Users,
        };
    }

    public override Task<User?> FirstOrDefaultAsync(
        QueryTrackerBehavior behavior,
        System.Linq.Expressions.Expression<Func<User, bool>> predicate,
        CancellationToken ct = default)
    {
        return GetQuery(behavior).FirstOrDefaultAsync(predicate, ct);
    }

    public override Task<User?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<User, bool>> predicate,
        CancellationToken ct = default)
    {
        return context.Users.FirstOrDefaultAsync(predicate, ct);
    }

    public override void Add(User user)
    {
        context.Users.Add(user);
    }
}
