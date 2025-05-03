using MediatR;

namespace Application.Events;

public class JsonCheckRequestedEvent : INotification
{
    public long SessionId { get; init; }
}

public class JsonCheckRequestedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<JsonCheckRequestedEvent>
{
    public async Task Handle(JsonCheckRequestedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(new Message()
            {
                ChatId = notification.SessionId,
                Id = session.LastSentMessageId,
                Text = "Paste a json file",
            }, cancellationToken);
        }
    }
}