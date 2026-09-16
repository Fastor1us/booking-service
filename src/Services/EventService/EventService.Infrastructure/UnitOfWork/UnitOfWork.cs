using EventService.Application.Interfaces;
using EventService.Infrastructure.Persistence;
using Messaging.Abstractions.Persistence;
using Messaging.Persistence.EfCore;

namespace EventService.Infrastructure.UnitOfWork;

public class UnitOfWork(
    AppDbContext context,
    IEventRepository eventRepository,
    IOutboxRepository outboxRepository,
    IOutboxDeadLetterRepository outboxDeadLetterRepository)
    : UnitOfWorkBase(context, outboxRepository, outboxDeadLetterRepository),
      IUnitOfWork
{
    public IEventRepository Events => eventRepository;
}
