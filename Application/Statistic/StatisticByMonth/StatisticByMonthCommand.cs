using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticByMonth;

public record StatisticByMonthCommand : IRequest
{
    public long SessionId { get; init; }
}

public class StatisticByMonthCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, ICategoryProvider categoryProvider, IMediator mediator)
    : IRequestHandler<StatisticByMonthCommand>
{
    public async Task Handle(StatisticByMonthCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session == null)
        {
            session = new UserSession(){Id = request.SessionId};
            userSessionService.SaveUserSession(session);
        }

        StatisticsFlow flow;
        session.ActiveFlow = flow = new StatisticsFlow(dateTimeService, categoryProvider);
        flow.Draft.Mode = StatisticsQueryMode.MonthlyExpenses; 
        
        await mediator.Publish(new StatisticByMonthCreatedEvent() { SessionId = session.Id }, cancellationToken);
    }
}