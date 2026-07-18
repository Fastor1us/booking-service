namespace BookingApi.Application.Interfaces;

/// <summary>
///     Marker interface for all repositories in the system.
///     All repository implementations must inherit from this interface
///     to be recognized by the Unit of Work.
/// </summary>
/// <remarks>
///     This is a contract that ensures all repositories share the same
///     data access patterns and can be managed by the Unit of Work.
/// </remarks>
public interface IRepository
{
}
