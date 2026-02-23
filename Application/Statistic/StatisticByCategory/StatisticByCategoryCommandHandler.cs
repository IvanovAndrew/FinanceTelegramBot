using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticByCategory;

public class StatisticByCategoryCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator)
    : IRequestHandler<StatisticByCategoryCommand>
{
    public async Task Handle(StatisticByCategoryCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session == null)
        {
            session = new UserSession(){Id = request.SessionId};
            userSessionService.SaveUserSession(session);
        }

        StatisticsFlow flow;
        session.ActiveFlow = flow = new StatisticsFlow(dateTimeService);
        flow.Draft.Mode = StatisticsQueryMode.CategoryByMonths;
            
        await mediator.Publish(new DraftUpdatedEvent() { SessionId = request.SessionId }, cancellationToken);
    }
}