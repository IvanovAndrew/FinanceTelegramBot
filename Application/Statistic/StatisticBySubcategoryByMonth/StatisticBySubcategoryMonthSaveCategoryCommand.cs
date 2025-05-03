using Application.Statistic.StatisticBySubcategoryByMonth;
using MediatR;

namespace Application.Commands.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryMonthSaveCategoryCommand : IRequest
{
    public long SessionId { get; init; }
    public string Category { get; init; }
}

public class StatisticBySubcategoryMonthSaveCategoryCommandHandler(
    IUserSessionService userSessionService,
    ICategoryProvider categoryProvider, 
    IMediator mediator) : IRequestHandler<StatisticBySubcategoryMonthSaveCategoryCommand>
{
    public async Task Handle(StatisticBySubcategoryMonthSaveCategoryCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            var category = categoryProvider.GetCategories(false)
                .FirstOrDefault(c => string.Equals(c.Name, request.Category));

            if (category != null)
            {
                session.StatisticsOptions.Category = category;
                session.QuestionnaireService.Next();

                await mediator.Publish(new StatisticBySubcategoryMonthCategorySavedEvent()
                    { SessionId = request.SessionId }, cancellationToken);
            }
        }
    }
}