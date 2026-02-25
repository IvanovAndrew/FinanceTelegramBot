using MediatR;

namespace Application.Events;

public record EnterDescriptionEvent : INotification
{
    public long SessionId { get; init; }
}

public class EnterDescriptionEventHandler(IConversation conversation) : INotificationHandler<EnterDescriptionEvent>
{
    public async Task Handle(EnterDescriptionEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.EnterDescription(), cancellationToken);
    }
}