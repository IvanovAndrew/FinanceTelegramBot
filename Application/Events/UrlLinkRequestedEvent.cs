using MediatR;

namespace Application.Events;

public record UrlLinkRequestedEvent : INotification
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
            session.QuestionnaireService = new RequisitesFromUrlQuestionnaireService();
            
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