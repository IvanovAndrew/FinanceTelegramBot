using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public class AskSubCategoryEventHandler(IConversation conversation, ILogger<AskSubCategoryEventHandler> logger)
    : INotificationHandler<EnterSubCategoryEvent>
{
    public async Task Handle(EnterSubCategoryEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(AskSubCategoryEventHandler)} started");
        
        await conversation.Update(notification.SessionId, Screens.SelectSubCategory(notification.SubCategories), cancellationToken);
        
        logger.LogInformation($"{nameof(AskSubCategoryEventHandler)} finished");
    }
}