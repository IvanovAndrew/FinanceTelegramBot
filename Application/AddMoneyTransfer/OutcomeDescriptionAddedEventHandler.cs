using MediatR;

namespace Application.AddMoneyTransfer;

public class OutcomeDescriptionAddedEventHandler(IMessageService messageService, IUserSessionService userSessionService)
    : INotificationHandler<OutcomeDescriptionAddedEvent>
{
    public async Task Handle(OutcomeDescriptionAddedEvent notification, CancellationToken cancellationToken)
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