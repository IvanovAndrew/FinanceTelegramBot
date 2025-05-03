using Application.Events;
using MediatR;

namespace Application.Statistic.StatisticByCategory;

public class StatisticByCategoryCommandHandler(IUserSessionService userSessionService, IMediator mediator)
    : IRequestHandler<StatisticByCategoryCommand>
{
    public async Task Handle(StatisticByCategoryCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            session.StatisticsOptions = new StatisticsOptions();
            session.QuestionnaireService = new CategoryStatisticQuestionnaire();
            
            await mediator.Publish(new StatisticByCategoryCreatedEvent { SessionId = request.SessionId }, cancellationToken);
        }
    }
}