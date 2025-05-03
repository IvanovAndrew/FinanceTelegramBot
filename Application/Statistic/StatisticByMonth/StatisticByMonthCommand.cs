using Domain.Events;
using MediatR;

namespace Application.Commands.StatisticByMonth;

public class StatisticByMonthCommand : IRequest
{
    public long SessionId { get; init; }
}

public class StatisticByMonthCommandHandler(IUserSessionService userSessionService, IMediator mediator)
    : IRequestHandler<StatisticByMonthCommand>
{
    public async Task Handle(StatisticByMonthCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            session.StatisticsOptions = new StatisticsOptions();
            session.QuestionnaireService = new MonthStatisticQuestionnaire();
            
            await mediator.Publish(new StatisticByMonthCreatedEvent() { SessionId = session.Id }, cancellationToken);
        }
    }
}