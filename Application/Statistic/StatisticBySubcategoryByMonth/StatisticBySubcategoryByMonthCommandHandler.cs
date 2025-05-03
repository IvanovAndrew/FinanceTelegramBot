using Application.Commands.StatisticBySubcategoryByMonth;
using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryByMonthCommandHandler(IUserSessionService userSessionService, IMediator mediator)
    : IRequestHandler<StatisticBySubcategoryByMonthCommand>
{
    public async Task Handle(StatisticBySubcategoryByMonthCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            session.StatisticsOptions = new StatisticsOptions();
            session.QuestionnaireService = new SubcategoryByMonthStatisticQuestionnaire();
        
            await mediator.Publish(new StatisticBySubcategoryByMonthCreatedEvent{SessionId = session.Id}, cancellationToken);
        }
    }
}