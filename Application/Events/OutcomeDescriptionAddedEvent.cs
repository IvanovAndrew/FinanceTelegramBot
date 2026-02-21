using Application.AddMoneyTransfer;
using MediatR;  

namespace Application.Events;

public class OutcomeDescriptionAddedEventHandler(IMessageService messageService, IUserSessionService userSessionService)
    : INotificationHandler<EnterThePriceEvent>
{
    public async Task Handle(EnterThePriceEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);
        if (session != null)
        {
            await messageService.DeleteMessageAsync(new Message(){ChatId = notification.SessionId, Id = session.LastSentMessageId!}, cancellationToken);                          
            var message = await messageService.SendTextMessageAsync(new Message()
            {
                ChatId = notification.SessionId, Text = "Enter the price"
            }, cancellationToken);             
            session.LastSentMessageId = message.Id;
        }
    } 
}