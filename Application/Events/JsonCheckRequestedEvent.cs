using MediatR;

namespace Application.Events;

public class JsonCheckRequestedEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
}

public class JsonCheckRequestedEventHandler(IMessageService messageService) : INotificationHandler<JsonCheckRequestedEvent>
{
    public async Task Handle(JsonCheckRequestedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Id = notification.LastSentMessageId,
            Text = "Paste a json file",
        }, cancellationToken);
    }
}