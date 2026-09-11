using Application.Bot.Flows;
using Application.Core;
using Application.Core.Services;
using MediatR;

namespace Application.Bot;

public record RequisitesRequestedEvent : INotification
{
    public long SessionId { get; init; }
}

public class RequisitesRequestedEventHandler(IUserSessionService userSessionService, IConversation conversation) : INotificationHandler<RequisitesRequestedEvent>
{
    public async Task Handle(RequisitesRequestedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await conversation.Update(notification.SessionId, Screens.SelectRequisiteSource(), cancellationToken);
        }
    }
}

public record YerevanCityRequestedEvent : INotification
{
    public long SessionId { get; init; }
}

public class YerevanCityRequestedEventHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator) : INotificationHandler<YerevanCityRequestedEvent>
{
    public async Task Handle(YerevanCityRequestedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            session.ActiveFlow = new YerevanCityCheckRequisiteFlow(dateTimeService);
            await mediator.Publish(new DraftUpdatedEvent { SessionId = notification.SessionId }, cancellationToken);
        }
    }
}

public record RussianShopRequestedEvent : INotification
{
    public long SessionId { get; init; }
}

public class RussianShopRequestedEventHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator) : INotificationHandler<RussianShopRequestedEvent>
{
    public async Task Handle(RussianShopRequestedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            session.ActiveFlow = new FnsCheckRequisiteFlow(dateTimeService);
            await mediator.Publish(new DraftUpdatedEvent { SessionId = notification.SessionId }, cancellationToken);
        }
    }
}

public record UrlLinkRequestedEvent : INotification
{
    public long SessionId { get; init; }
}

public class UrlLinkRequestedEventHandler(IUserSessionService userSessionService, IMediator mediator) : INotificationHandler<UrlLinkRequestedEvent>
{
    public async Task Handle(UrlLinkRequestedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            session.ActiveFlow = new FnsCheckLinkFlow();
            await mediator.Publish(new DraftUpdatedEvent { SessionId = notification.SessionId }, cancellationToken);
        }
    }
}