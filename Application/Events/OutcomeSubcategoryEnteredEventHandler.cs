using MediatR;

namespace Application.Events;

public class OutcomeSubcategoryEnteredEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<OutcomeSubсategoryEnteredEvent>
{
    public async Task Handle(OutcomeSubсategoryEnteredEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(new Message()
            {
                ChatId = notification.SessionId,
                Id = session.LastSentMessageId,
                Text = "Enter the description",
            }, cancellationToken: cancellationToken);
        }
    }
}