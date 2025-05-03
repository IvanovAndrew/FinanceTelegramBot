using Domain.Events;
using MediatR;

namespace Application.Commands.StatisticByDay;

public class StatisticByDayCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<StatisticByDayCommand>
{
    public async Task Handle(StatisticByDayCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            session.QuestionnaireService = new DayStatisticQuestionnaire();
            session.StatisticsOptions = new StatisticsOptions();
            await mediator.Publish(new StatisticByDayCreatedEvent { SessionId = request.SessionId }, cancellationToken);
        }
    }
}