using EventService.Domain.Models;

namespace EventService.Application.Tests.Helpes;

public static class Extensions
{
    public static bool IsEqual(this Event actual, Event expected)
    {
        return expected.Id == actual.Id &&
            expected.Title == actual.Title &&
            expected.StartAt == actual.StartAt &&
            expected.EndAt == actual.EndAt;
    }
}
