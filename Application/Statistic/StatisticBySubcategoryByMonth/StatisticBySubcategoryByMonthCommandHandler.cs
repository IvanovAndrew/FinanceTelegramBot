using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryByMonthCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator)
    : IRequestHandler<StatisticBySubcategoryByMonthCommand>
{
    public async Task Handle(StatisticBySubcategoryByMonthCommand request, CancellationToken cancellationToken)
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

        flow.Draft.Mode = StatisticsQueryMode.SubcategoryByMonth;
        
        await mediator.Publish(new DraftUpdatedEvent(){SessionId = session.Id}, cancellationToken);
    }
}