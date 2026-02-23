using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public class AskSubCategoryEventHandler(IMessageService messageService, ILogger<AskSubCategoryEventHandler> logger)
    : INotificationHandler<EnterSubCategoryEvent>
{
    public async Task Handle(EnterSubCategoryEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(AskSubCategoryEventHandler)} started");
        
        await messageService.EditSentTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Id = notification.LastSentMessageId,
            Text = "Enter the subcategory",
            Options = MessageOptions.FromList(notification.Category.Subcategories.Select(c => new Option(c.Code, c.Name)).ToList()),
        }, cancellationToken: cancellationToken);
        
        
        logger.LogInformation($"{nameof(AskSubCategoryEventHandler)} finished");
    }
}