using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public class AskCategoryEventHandler(
    IMessageService messageService,
    ILogger<AskCategoryEventHandler> logger)
    : INotificationHandler<EnterCategoryEvent>
{
    public async Task Handle(EnterCategoryEvent notification, CancellationToken cancellationToken)
    {
        var options = notification.Categories.Select(c => new Option(c.Code, c.ShortName ?? c.Name)).ToList();
        
        var newMessageToSend = new Message()
        {
            ChatId = notification.SessionId,
            Id = notification.LastSentMessageId,
            Text = "Enter the category",
            Options = MessageOptions.FromList(options)
        };

        await messageService.EditSentTextMessageAsync(newMessageToSend, cancellationToken);
    }
}