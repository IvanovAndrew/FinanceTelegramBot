using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, ICategoryProvider categoryProvider, IMediator mediator) : IRequestHandler<StatisticBalanceCommand>
{
    public async Task Handle(StatisticBalanceCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session == null)
        {
            session = new UserSession(){Id = request.SessionId}; 
            userSessionService.SaveUserSession(session);
        }

        if (session.ActiveFlow is not StatisticsFlow flow)
        {
            session.ActiveFlow = flow = new StatisticsFlow(dateTimeService, categoryProvider);
        }

        flow.Draft.Mode = StatisticsQueryMode.BalanceFromMonth;

        await mediator.Publish(new DraftUpdatedEvent() { SessionId = session.Id}, cancellationToken);
    }
}