using Messaging.Abstractions.Outbox;

namespace BookingService.Application.Interfaces;

public interface IOutboxDeadLetterRepository : IRepository<OutboxDeadLetter>
{
}
