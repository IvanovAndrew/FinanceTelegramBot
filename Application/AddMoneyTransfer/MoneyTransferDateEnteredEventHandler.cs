using MediatR;

namespace Application.AddMoneyTransfer;

public class MoneyTransferDateEnteredEventHandler(
    IMessageService messageService,
    ICategoryProvider categoryProvider,
    IUserSessionService userSessionService)
    : INotificationHandler<MoneyTransferDateEnteredEvent>
{
    public async Task Handle(MoneyTransferDateEnteredEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            var newMessageToSend = new Message()
            {
                ChatId = session.Id,
                Id = session.LastSentMessageId!,
                Text = "Enter the category",
                Options = MessageOptions.FromList(categoryProvider.GetCategories(session.MoneyTransferBuilder.IsIncome).Select(c => c.ShortName?? c.Name).ToList())
            };

            await messageService.EditSentTextMessageAsync(newMessageToSend, cancellationToken);
        }
    }
}