using MediatR;

namespace Application.Events;

public record CheckOutcomeQuestionnaireRequestedEvent : INotification
{
    public long SessionId { get; init; }
}

public class CheckOutcomeQuestionnaireRequestedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<
    CheckOutcomeQuestionnaireRequestedEvent>
{
    public async Task Handle(CheckOutcomeQuestionnaireRequestedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = session.Id,
                    Id = session.LastSentMessageId,
                    Text = "Enter the check",
                    Options = MessageOptions.FromList( 
                        new []
                        {
                            new Option("/json", "json"),
                            new Option("/url", "Url Link"),
                            new Option("/requisites", "By Requisites"),
                        })
                }, cancellationToken);
        }
    }
}