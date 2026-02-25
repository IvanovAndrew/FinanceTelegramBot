using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class RequisitesCreatedEvent : INotification
{
    public long SessionId { get; init; }
}

public class RequisitesCreatedEventHandler(IConversation conversation) : INotificationHandler<RequisitesCreatedEvent>
{
    public async Task Handle(RequisitesCreatedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.EnterDateTime("Enter the check date and time"), cancellationToken);
    }
}