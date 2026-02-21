using MediatR;

namespace Application.Events;

public class ConfirmEvent : INotification
{
    public long SessionId { get; init; }
}

public class OutcomePriceAddedEventHandler(IUserSessionService userSessionService, IMessageService messageService)
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
        
        await messageService.DeleteMessageAsync(new Message(){ChatId = notification.SessionId, Id = session.LastSentMessageId}, cancellationToken);
        
        var lastSentMessage = await messageService.SendTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = string.Join($"{Environment.NewLine}",
                moneyTransfer.ToString(),
                "",
                "Would you like to save it?"
            ),
            Options = MessageOptions.FromList([new Option("/save", "Save"), new Option("/cancel", "Cancel")]),
        }, cancellationToken: cancellationToken);

        session.LastSentMessageId = lastSentMessage.Id;
    }
}