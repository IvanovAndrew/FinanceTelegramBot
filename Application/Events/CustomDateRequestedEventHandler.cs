using MediatR;

namespace Application.Events;

public class CustomDateRequestedEventHandler(IUserSessionService userSessionService, IMessageService messageService)  : INotificationHandler<CustomDateRequestedEvent>
{
    public async Task Handle(CustomDateRequestedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Id = notification.LastSentMessageId,
            Text = notification.Text
        }, cancellationToken);
    }
}