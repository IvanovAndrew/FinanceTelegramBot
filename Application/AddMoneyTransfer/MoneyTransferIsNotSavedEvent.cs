using MediatR;

namespace Application.AddMoneyTransfer;

public record MoneyTransferIsNotSavedEvent : INotification
{
    public long SessionId { get; init; }
    public string Reason { get; init; }
}

public class MoneyTransferIsNotSavedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<MoneyTransferIsNotSavedEvent>
{
    public async Task Handle(MoneyTransferIsNotSavedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(new Message()
            { 
                ChatId = notification.SessionId, 
                Id = session.LastSentMessageId, 
                Text = string.Join($"{Environment.NewLine}",
                    "Couldn't save expense",
                    notification.Reason)
            }, cancellationToken);

            userSessionService.RemoveSession(session.Id);
        }
    }
}