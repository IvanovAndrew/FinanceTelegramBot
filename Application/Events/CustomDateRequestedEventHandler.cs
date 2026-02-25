using MediatR;

namespace Application.Events;

public class CustomDateRequestedEventHandler(IConversation conversation)  : INotificationHandler<CustomDateRequestedEvent>
{
    public async Task Handle(CustomDateRequestedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.EnterText(notification.Text), cancellationToken);
    }
}