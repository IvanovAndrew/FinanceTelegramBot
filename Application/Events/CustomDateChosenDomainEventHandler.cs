using Application.AddMoneyTransfer;
using MediatR;

namespace Application.Events;

public class CustomDateChosenEventHandler(IDateTimeService dateTimeService, IMessageService messageService) : INotificationHandler<CustomDateChosenEvent>
{
    public async Task Handle(CustomDateChosenEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Text = $"Enter the date. Example: {dateTimeService.Today().ToString(DateFormat.Day)}"
            },
            cancellationToken);
    }
}