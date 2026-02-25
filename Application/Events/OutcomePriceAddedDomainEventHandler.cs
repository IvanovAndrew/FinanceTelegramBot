using MediatR;

namespace Application.Events;

public class ConfirmEvent : INotification
{
    public long SessionId { get; init; }
}

public class OutcomePriceAddedEventHandler(IUserSessionService userSessionService, IConversation conversation)
    : INotificationHandler<ConfirmEvent>
{
    public async Task Handle(ConfirmEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session?.ActiveFlow is not AddMoneyTransferFlow flow)
        {
            return;
        }
        
        var moneyTransfer = flow.ToEntity();
        await conversation.Update(notification.SessionId, Screens.Confirm(string.Join($"{Environment.NewLine}",
                moneyTransfer.ToString(),
                "",
                "Would you like to save it?"
            )), cancellationToken);
    }
}