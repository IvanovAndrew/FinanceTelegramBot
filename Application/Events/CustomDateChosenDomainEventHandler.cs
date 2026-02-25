using Application.AddMoneyTransfer;
using MediatR;

namespace Application.Events;

public class CustomDateChosenEventHandler(IDateTimeService dateTimeService, IConversation conversation) : INotificationHandler<CustomDateChosenEvent>
{
    public async Task Handle(CustomDateChosenEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId,
            Screens.SelectDay(dateTimeService.Today()), cancellationToken);
    }
}