using MediatR;

namespace Application.Events;

public record EnterUrlLinkEvent : INotification
{
    public long SessionId { get; init; }
}

public class EnterUrlLinkEventHandler(IConversation conversation) : INotificationHandler<EnterUrlLinkEvent>
{
    public async Task Handle(EnterUrlLinkEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.EnterText("Enter the url"), cancellationToken);
    }
}