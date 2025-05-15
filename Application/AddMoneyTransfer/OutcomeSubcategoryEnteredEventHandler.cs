using Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public class OutcomeSubcategoryEnteredEventHandler(IUserSessionService userSessionService, IMessageService messageService, ILogger<OutcomeSubcategoryEnteredEventHandler> logger) : INotificationHandler<OutcomeSubcategoryEnteredEvent>
{
    public async Task Handle(OutcomeSubcategoryEnteredEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(OutcomeSubcategoryEnteredEventHandler)} started");
        
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(new Message()
            {
                ChatId = notification.SessionId,
                Id = session.LastSentMessageId,
                Text = "Enter the description",
            }, cancellationToken: cancellationToken);
        }
        else
        {
            logger.LogWarning($"Session with id {notification.SessionId} has not been found");
        }
        
        logger.LogInformation($"{nameof(OutcomeSubcategoryEnteredEventHandler)} finished");
    }
}