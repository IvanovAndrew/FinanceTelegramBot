using Application.AddMoneyTransfer;
using MediatR;

namespace Application.Events;

public class OutcomeSavedEventHandler(IUserSessionService userSessionService, IMessageService messageService)
    : INotificationHandler<MoneyTransferSavedEvent>
{
    public async Task Handle(MoneyTransferSavedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(new Message()
                { 
                    ChatId = notification.SessionId, 
                    Id = session.LastSentMessageId, 
                    Text = string.Join($"{Environment.NewLine}",
                        notification.MoneyTransfer.ToString(),
                        "", 
                        "Saved")
                }, cancellationToken);

            userSessionService.RemoveSession(session.Id);
        }
    }
}