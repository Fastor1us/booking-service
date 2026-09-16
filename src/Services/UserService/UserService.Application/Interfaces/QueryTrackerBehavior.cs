namespace UserService.Application.Interfaces;

/// <summary>
/// Defines how entities are tracked by the data context during queries.
/// </summary>
public enum QueryTrackerBehavior
{
    /// <summary>
    /// Entities are tracked.
    /// Changes to entities will be persisted when SaveChanges is called.
    /// Use this when you plan to modify and update entities.
    /// </summary>
    Track,

    /// <summary>
    /// Entities are not tracked.
    /// Better performance for read-only operations.
    /// Changes to entities will NOT be persisted.
    /// </summary>
    NoTracking,

    /// <summary>
    /// Entities are not tracked but identity resolution is used.
    /// Prevents multiple instances of the same entity in the result set.
    /// Good for complex queries where you might get same entity multiple times.
    /// </summary>
    NoTrackingWithIdentityResolution
}
