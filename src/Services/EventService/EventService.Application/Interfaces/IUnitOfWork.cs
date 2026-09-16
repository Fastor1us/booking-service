using Messaging.Abstractions.Persistence;

namespace EventService.Application.Interfaces;

public interface IUnitOfWork : IUnitOfWorkBase
{
    IEventRepository Events { get; }
}
