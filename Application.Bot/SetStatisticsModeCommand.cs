using Application.Bot;
using Application.Bot.Flows;
using Application.Core;
using Application.Core.Services;
using MediatR;

public record SetStatisticsModeCommand : IRequest
{
    public long SessionId { get; init; }
    public StatisticsQueryMode Mode { get; init; }
}

public class SetStatisticsModeCommandHandler(IUserSessionService sessions, IDateTimeService dt, IMediator mediator)
    : IRequestHandler<SetStatisticsModeCommand>
{
    public async Task Handle(SetStatisticsModeCommand r, CancellationToken ct)
    {
        var session = sessions.GetUserSession(r.SessionId) ?? new UserSession { Id = r.SessionId };
        sessions.SaveUserSession(session);

        if (session.ActiveFlow is not StatisticsFlow flow)
            session.ActiveFlow = flow = new StatisticsFlow(dt);

        flow.Draft.Mode = r.Mode;
        await mediator.Publish(new DraftUpdatedEvent { SessionId = r.SessionId }, ct);
    }
}