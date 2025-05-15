using MediatR;

namespace Application.AddMoneyTransfer;

public class OutcomePriceAddedEventHandler(IUserSessionService userSessionService, IMessageService messageService)
    : INotificationHandler<OutcomePriceAddedEvent>
{
    public async Task Handle(OutcomePriceAddedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            var moneyTransfer = session.MoneyTransferBuilder.Build();
            
            await messageService.DeleteMessageAsync(new Message(){ChatId = notification.SessionId, Id = session.LastSentMessageId}, cancellationToken);
            var lastSentMessage = await messageService.SendTextMessageAsync(new Message()
            {
                ChatId = notification.SessionId,
                Text = string.Join($"{Environment.NewLine}",
                    moneyTransfer.ToString(),
                    "",
                    "Would you like to save it?"
                ),
                Options = MessageOptions.FromList(new[] { new Option("/save", "Save"), new Option("/cancel", "Cancel") }),
            }, cancellationToken: cancellationToken);

            session.LastSentMessageId = lastSentMessage.Id;
        }
    }
}