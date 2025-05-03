using MediatR;

namespace Application.Events;

public class UrlLinkRequestedEvent : INotification
{
    public long SessionId { get; init; }
}

public class UrlLinkRequestedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<UrlLinkRequestedEvent>
{
    public async Task Handle(UrlLinkRequestedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = session.Id,
                    Id = session.LastSentMessageId,
                    Text = "Enter the url",
                }, cancellationToken);
        }
    }
}