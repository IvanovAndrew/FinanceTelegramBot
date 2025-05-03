using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public class StatisticBySubcategorySaveCategoryCommandHandler(IUserSessionService userSessionService, ICategoryProvider categoryProvider, IMediator mediator) : IRequestHandler<StatisticBySubcategorySaveCategoryCommand>
{
    public async Task Handle(StatisticBySubcategorySaveCategoryCommand request, CancellationToken cancellationToken)
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

                await mediator.Publish(new StatisticBySubcategoryCategorySavedEvent(){SessionId = session.Id}, cancellationToken);
            }
        }
    }
}