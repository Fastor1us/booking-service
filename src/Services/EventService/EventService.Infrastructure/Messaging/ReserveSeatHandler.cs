using EventService.Application.Interfaces;
using Messaging.Abstractions;

namespace EventService.Infrastructure.Messaging;

public class ReserveSeatHandler(IUnitOfWork unitOfWork) : IMessageHandler
{
    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        // TODO:

        // Inbox pattern
        // check ID exist / if not - add
    }
}
