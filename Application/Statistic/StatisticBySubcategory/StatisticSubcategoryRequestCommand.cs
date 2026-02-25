using Domain;
using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public class StatisticSubcategoryRequestCommand : IRequest
{
    public long SessionId { get; init; }
    public StatisticsQuery Query { get; init; }
}

public class StatisticSubcategoryRequestCommandHandler(IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<StatisticSubcategoryRequestCommand>
{
    public async Task Handle(StatisticSubcategoryRequestCommand request, CancellationToken cancellationToken)
    {
        var query = request.Query;
    
        var filter = new FinanceFilter()
        {
            DateFrom = query.MonthRange.From.ToDateOnly(),
            Category = query.Category,
            Subcategory = query.SubCategory,
            Currency = query.Currency,
        };

        var outcomes = await financeRepository.ReadOutcomes(filter, cancellationToken);

        if (outcomes.Any())
        {
            await mediator.Publish(new StatisticSubcategoryExpensesLoadedEvent()
                { 
                    SessionId = request.SessionId,
                    
                    Category = filter.Category,
                    SubCategory = filter.Subcategory,
                    
                    MonthFrom = query.MonthRange.From,
                    Outcomes = outcomes
                }, 
                cancellationToken);
        }
    }
}