using MediatR;

namespace Application.Events;

public record EnterDescriptionEvent : INotification
{
    public long SessionId { get; init; }
}

public class EnterDescriptionEventHandler(IMessageService messageService) : INotificationHandler<EnterDescriptionEvent>
{
    public async Task Handle(EnterDescriptionEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = "Enter the description",
        }, cancellationToken: cancellationToken);
    }
}