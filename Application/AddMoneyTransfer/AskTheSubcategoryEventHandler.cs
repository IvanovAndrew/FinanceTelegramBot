using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public class AskTheSubcategoryEventHandler(IMessageService messageService, IUserSessionService userSessionService, ILogger<AskTheSubcategoryEventHandler> logger)
    : INotificationHandler<EnterSubcategoryEvent>
{
    public async Task Handle(EnterSubcategoryEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(AskTheSubcategoryEventHandler)} started");
        
        await messageService.EditSentTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Id = notification.LastSentMessageId,
            Text = "Enter the subcategory",
            Options = MessageOptions.FromList(notification.Category.Subcategories.Select(c => c.Name).ToList()),
        }, cancellationToken: cancellationToken);
        
        
        logger.LogInformation($"{nameof(AskTheSubcategoryEventHandler)} finished");
    }
}