using Application.AddMoneyTransfer;
using MediatR;

namespace Application.Events;

public class CustomDateChosenEventHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMessageService messageService) : INotificationHandler<CustomDateChosenEvent>
{
    public async Task Handle(CustomDateChosenEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = session.Id,
                    Id = notification.LastSentMessageId,
                    Text = $"Enter the date. Example: {dateTimeService.Today().ToString("dd MMMM yyyy")}"
                },
                cancellationToken);
        }
    }
}