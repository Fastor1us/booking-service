using Messaging.Abstractions.Persistence;

namespace EventService.Application.Interfaces;

public interface IUnitOfWork : IUnitOfWorkBase
{
    IEventRepository EventRepository { get; }
}
