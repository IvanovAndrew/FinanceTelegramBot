using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class RequisitesRequestedEvent : INotification
{
    public long SessionId { get; init; }
}

public class RequisitesRequestedEventHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator) : INotificationHandler<RequisitesRequestedEvent>
{
    public async Task Handle(RequisitesRequestedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            session.ActiveFlow = new CheckRequisiteFlow(dateTimeService);

            await mediator.Publish(new EnterDateTimeEvent() { SessionId = notification.SessionId }, cancellationToken);
        }
    }
}