using Domain.Events;
using MediatR;

namespace Application.Statistic.StatisticByCategory;

public class StatisticByCategorySaveCategoryCommand : IRequest
{
    public long SessionId { get; init; }
    public string Category { get; init; }
}

public class StatisticByCategorySaveCategoryCommandHandler(IUserSessionService userSessionService, ICategoryProvider categoryProvider, IMediator mediator) : IRequestHandler<StatisticByCategorySaveCategoryCommand>
{
    public async Task Handle(StatisticByCategorySaveCategoryCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            var category = categoryProvider.GetCategories(false).FirstOrDefault(c => string.Equals(c.Name, request.Category, StringComparison.InvariantCultureIgnoreCase));

            if (category != null)
            {
                session.StatisticsOptions.Category = category;
                session.QuestionnaireService.Next();

                await mediator.Publish(new StatisticByCategorySaveCategorySavedEvent() { SessionId = session.Id }, cancellationToken);
            }
        }
    }
}