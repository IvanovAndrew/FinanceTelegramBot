using MediatR;

namespace Application.Events;

public class UserStartedEventHandler(IConversation conversation) : INotificationHandler<UserStartedEvent>
{
    public async Task Handle(UserStartedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionID, Screens.SelectStart(), cancellationToken);
    }
}