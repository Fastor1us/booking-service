using BookingService.Application.Interfaces;

namespace BookingService.Application.Messaging;

public interface IOutboxRepository : IRepository<OutboxMessage>
{
    //void Add(
    //    Guid id,
    //    string topic,
    //    string key,
    //    string messageType,
    //    Guid correlationId,
    //    string payload,
    //    DateTimeOffset publishedAtUtc);
}
