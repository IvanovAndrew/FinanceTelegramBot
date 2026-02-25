using Application.AddMoneyTransfer;
using MediatR;  

namespace Application.Events;

public class OutcomeDescriptionAddedEventHandler(IConversation conversation)
    : INotificationHandler<EnterThePriceEvent>
{
    public async Task Handle(EnterThePriceEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.EnterPrice(), cancellationToken);
    } 
}