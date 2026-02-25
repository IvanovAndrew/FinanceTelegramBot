using MediatR;

namespace Application.AddMoneyTransfer;

public class IncomeCreatedEventHandler(IConversation conversation, IDateTimeService dateTimeService)
    : INotificationHandler<IncomeCreatedEvent>
{
    public async Task Handle(IncomeCreatedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, 
            Screens.SelectDay(dateTimeService.Today()), cancellationToken);
    }
}