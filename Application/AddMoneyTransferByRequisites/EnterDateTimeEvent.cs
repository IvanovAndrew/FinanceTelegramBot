using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class EnterDateTimeEvent : INotification
{
    public long SessionId { get; init; }
}

public class DateTimeEventHandler(IConversation conversation) : INotificationHandler<EnterDateTimeEvent>
{
    public async Task Handle(EnterDateTimeEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.EnterDateTime("Enter the check date and time"), cancellationToken);
    }
}