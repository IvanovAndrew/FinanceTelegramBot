using MediatR;

namespace Application.Events;

public class UserStartedEventHandler(IMessageService messageService, IUserSessionService userSessionService) : INotificationHandler<UserStartedEvent>
{
    public async Task Handle(UserStartedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionID);

        if (session != null)
        {
            var sentMessage = await messageService.SendTextMessageAsync(new Message()
            {
                ChatId = notification.SessionID,
                Text = "What would you like to do?",
                Options = MessageOptions.FromList(new[] { new Option("/outcome", "Outcome"), new Option("/income", "Income"), new Option("/statistics", "Statistics") })
            }, cancellationToken);

            session.LastSentMessageId = sentMessage.Id;
        }
    }
}