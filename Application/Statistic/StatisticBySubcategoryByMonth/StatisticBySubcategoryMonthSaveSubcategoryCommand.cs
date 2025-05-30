using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryMonthSaveSubcategoryCommand : IRequest
{
    public long SessionId { get; init; }
    public string Subcategory { get; init; }
}

public class
    StatisticBySubcategoryMonthSaveSubcategoryCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<StatisticBySubcategoryMonthSaveSubcategoryCommand>
{
    public async Task Handle(StatisticBySubcategoryMonthSaveSubcategoryCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session?.StatisticsOptions != null)
        {
            var category = session.StatisticsOptions.Category;
            var subcategory = category.GetSubcategoryByName(request.Subcategory);

            if (subcategory != null)
            {
                session.StatisticsOptions.Subcategory = subcategory;
                session.QuestionnaireService.Next();

                await mediator.Publish(
                    new StatisticBySubcategoryMonthSubcategorySavedEvent() { SessionId = session.Id },
                    cancellationToken);
            }
        }
    }
}