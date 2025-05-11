using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class RequisitesRequestedDomainEvent : INotification
{
    public long SessionId { get; init; }
}

public class RequisitesRequestedDomainEventHandler(IUserSessionService userSessionService, IMediator mediator) : INotificationHandler<RequisitesRequestedDomainEvent>
{
    public async Task Handle(RequisitesRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            session.QuestionnaireService = new ManualRequisitesQuestionnaireService();
            session.CheckRequisites = new CheckRequisite();

            await mediator.Publish(new RequisitesCreatedEvent()
                { SessionId = session.Id, LastSentMessageId = session.LastSentMessageId }, cancellationToken);
        }
    }
}

public class RequisitesCreatedEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class RequisitesCreatedEventHandler(IMessageService messageService) : INotificationHandler<RequisitesCreatedEvent>
{
    public async Task Handle(RequisitesCreatedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Id = notification.LastSentMessageId,
            Text = "Enter the check date and time"
        }, cancellationToken);
    }
}