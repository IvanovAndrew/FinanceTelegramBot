using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public class AskCategoryEventHandler(
    IMessageService messageService,
    ICategoryProvider categoryProvider,
    ILogger<AskCategoryEventHandler> logger)
    : INotificationHandler<EnterCategoryEvent>
{
    public async Task Handle(EnterCategoryEvent notification, CancellationToken cancellationToken)
    {
        var newMessageToSend = new Message()
        {
            ChatId = notification.SessionId,
            Id = notification.LastSentMessageId,
            Text = "Enter the category",
            Options = MessageOptions.FromList(categoryProvider.GetCategories(notification.IsIncome).Select(c => c.ShortName?? c.Name).ToList())
        };

        await messageService.EditSentTextMessageAsync(newMessageToSend, cancellationToken);
    }
}