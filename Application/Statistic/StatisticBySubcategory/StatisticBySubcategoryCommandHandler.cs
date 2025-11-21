using Application.Statistic;
using Application.Statistic.StatisticBySubcategory;
using MediatR;

namespace Application.Commands.StatisticBySubcategory;

public class StatisticBySubcategoryCommandHandler(IUserSessionService userSessionService, IMediator mediator)
    : IRequestHandler<StatisticBySubcategoryCommand>
{
    public async Task Handle(StatisticBySubcategoryCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            session.StatisticsOptions = new StatisticsOptions();
            session.QuestionnaireService = new SubcategoryStatisticQuestionnaire();
            
            await mediator.Publish(new StatisticBySubcategoryCreatedEvent { SessionId = request.SessionId }, cancellationToken);
        }
    }
}