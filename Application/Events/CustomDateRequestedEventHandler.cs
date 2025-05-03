using MediatR;

namespace Application.Events;

public class CustomDateRequestedEventHandler(IMessageService messageService)  : INotificationHandler<CustomDateRequestedEvent>
{
    public async Task Handle(CustomDateRequestedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = notification.Text
        }, cancellationToken);
    }
}