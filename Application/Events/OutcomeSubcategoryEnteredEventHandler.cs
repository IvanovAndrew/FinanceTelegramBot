using Application.AddMoneyTransfer;
using MediatR;

namespace Application.Events;

public class OutcomeSubcategoryEnteredEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<OutcomeSubCategoryEnteredEvent>
{
    public async Task Handle(OutcomeSubCategoryEnteredEvent notification, CancellationToken cancellationToken)
    {
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
    }
}