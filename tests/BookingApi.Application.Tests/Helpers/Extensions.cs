using BookingApi.Domain.Models;

namespace BookingApi.Application.Tests.Helpers;

public static class Extensions
{
    public static bool IsEqual(this Event actual, Event expected)
    {
        return expected.Id == actual.Id &&
            expected.Title == actual.Title &&
            expected.StartAt == actual.StartAt &&
            expected.EndAt == actual.EndAt;
    }

    public static bool IsEqual(this Booking actual, Booking expected)
    {
        return expected.Id == actual.Id &&
            expected.EventId == actual.EventId &&
            expected.Status == actual.Status &&
            expected.CreatedAt == actual.CreatedAt &&
            expected.ProcessedAt == actual.ProcessedAt;
    }
}
