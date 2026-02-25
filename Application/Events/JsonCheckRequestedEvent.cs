using MediatR;

namespace Application.Events;

public class JsonCheckRequestedEvent : INotification
{
    public long SessionId { get; init; }
}

public class JsonCheckRequestedEventHandler(IConversation conversation) : INotificationHandler<JsonCheckRequestedEvent>
{
    public async Task Handle(JsonCheckRequestedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.EnterText("Paste a json file"), cancellationToken);
    }
}