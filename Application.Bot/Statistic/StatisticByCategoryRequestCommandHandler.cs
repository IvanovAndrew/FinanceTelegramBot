using Application.Bot.Flows;
using Application.Core;
using Application.Core.Statistic;
using Domain;
using MediatR;

namespace Application.Bot.Statistic;

public record GetStatisticCategoryRequestCommand : IRequest
{
    public long SessionId { get; init; }
    public StatisticsQuery Query { get; init; }
}

public class GetStatisticCategoryRequestCommandHandler(IFinanceRepository repo, IMediator mediator)
    : StatisticQueryHandlerBase<GetStatisticCategoryRequestCommand>(repo, mediator)
{
    protected override long GetSessionId(GetStatisticCategoryRequestCommand r) => r.SessionId;

    protected override FinanceFilter BuildFilter(GetStatisticCategoryRequestCommand r) => new()
    {
        DateFrom = r.Query.MonthRange.From.ToDateOnly(), Category = r.Query.Category, Currency = r.Query.Currency
    };

    protected override INotification BuildReadEvent(GetStatisticCategoryRequestCommand r,
        IReadOnlyList<Outcome> outcomes)
    {
        var currencies = outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
        
        var expenseAggregator = new ExpensesAggregator<DateOnly>(e => e.Date, sortByMoney: false, sortAsc: true);
        var statistic = expenseAggregator.Aggregate(outcomes, outcomes.Select(c => c.Amount.Currency).Distinct().ToArray());
        var wrapper = StatisticMapper.Map(statistic, new DateOnlyColumnFactory());
        
        return new StatisticOutcomesReadEvent()
        {
            SessionId = r.SessionId,

            Subtitle = $"Category: {r.Query.Category.Name}",
            FirstColumnName = "Month",
            Statistic = wrapper,
            
            DiagramTitle = $"Category: {r.Query.Category.Name}",
            ChartPoints = statistic.Rows
                    .Select(r => (ChartBucket.ForMonth(YearMonth.From(r.Row)),
                    (IReadOnlyDictionary<Currency, Money>)currencies.ToDictionary(c => c, c => r[c])))
                        .ToList()
        };
    }
}

