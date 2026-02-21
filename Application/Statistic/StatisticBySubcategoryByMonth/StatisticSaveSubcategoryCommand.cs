using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public record StatisticSaveSubcategoryCommand : IRequest
{
    public long SessionId { get; init; }
    public string Subcategory { get; init; }
}

public class
    StatisticSaveSubcategoryCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<StatisticSaveSubcategoryCommand>
{
    public async Task Handle(StatisticSaveSubcategoryCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session?.ActiveFlow is not StatisticsFlow flow) return;
        
        var category = flow.Draft.Category;
        var subcategory = category.GetSubcategoryByName(request.Subcategory);

        if (subcategory != null)
        {
            flow.Draft.SetSubCategory(subcategory);

            await mediator.Publish(new DraftUpdatedEvent() { SessionId = session.Id }, cancellationToken);
        }
    }
}