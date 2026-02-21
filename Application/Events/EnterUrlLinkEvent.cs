using MediatR;

namespace Application.Events;

public record EnterUrlLinkEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class EnterUrlLinkEventHandler(IMessageService messageService) : INotificationHandler<EnterUrlLinkEvent>
{
    public async Task Handle(EnterUrlLinkEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Text = "Enter the url",
            }, cancellationToken);
    }
}