using MediatR;

namespace Application.AddMoneyTransfer;

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