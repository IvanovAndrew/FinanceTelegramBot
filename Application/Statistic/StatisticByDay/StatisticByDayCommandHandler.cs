using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticByDay;

public class StatisticByDayCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator) : IRequestHandler<StatisticByDayCommand>
{
    public async Task Handle(StatisticByDayCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);
        
        if (session == null)
        {
            session = new UserSession(){Id = request.SessionId};
            userSessionService.SaveUserSession(session);
        }

        if (session?.ActiveFlow is not StatisticsFlow flow)
        {
            session.ActiveFlow = flow = new StatisticsFlow(dateTimeService);
        }

        flow.Draft.Mode = StatisticsQueryMode.DailyExpenses;
        await mediator.Publish(new DraftUpdatedEvent() { SessionId = request.SessionId }, cancellationToken);
    }
}