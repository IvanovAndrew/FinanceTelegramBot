using Application.Core;
using Application.Core.AddMoneyTransfer;
using MediatR;

namespace Application.Bot;

public class OutcomeCreatedDomainEventHandler(IConversation conversation)
    : INotificationHandler<OutcomeCreatedEvent>
{
    public async Task Handle(OutcomeCreatedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId,
            Screens.EnterTheSource(), 
            cancellationToken);
    }
}