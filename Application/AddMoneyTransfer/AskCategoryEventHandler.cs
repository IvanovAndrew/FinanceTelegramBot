using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public class AskCategoryEventHandler(
    IConversation conversation,
    ILogger<AskCategoryEventHandler> logger)
    : INotificationHandler<EnterCategoryEvent>
{
    public async Task Handle(EnterCategoryEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.SelectCategory(notification.Categories), cancellationToken);
    }
}