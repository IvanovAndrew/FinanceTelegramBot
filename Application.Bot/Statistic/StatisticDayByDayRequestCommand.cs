using Application.Core;
using Domain;
using MediatR;

namespace Application.Bot.Statistic;

public record StatisticDayByDayRequestCommand : IRequest
{
    public long SessionId { get; init; }
    public StatisticsQuery Query { get; init; }
}

public class StatisticDayByDayRequestCommandHandler(IExpensesService expensesService, IMediator mediator) :
    StatisticQueryHandlerBase<StatisticDayByDayRequestCommand>(expensesService, mediator)
{
    protected override long GetSessionId(StatisticDayByDayRequestCommand request)
    {
        return request.SessionId;
    }

    protected override FinanceFilter BuildFilter(StatisticDayByDayRequestCommand request)
    {
        return new FinanceFilter()
        {
            DateFrom = request.Query.MonthRange.From.ToDateOnly(),
            Currency = request.Query.Currency,
        };
    }

    protected override INotification BuildReadEvent(StatisticDayByDayRequestCommand request, IReadOnlyList<Outcome> outcomes)
    {
        var chartPoints = outcomes
            .GroupBy(o => o.Date)
            .OrderBy(g => g.Key)
            .Select(g => (ChartBucket.ForDay(g.Key), (IReadOnlyDictionary<Currency, Money>)new Dictionary<Currency, Money>
            {
                [request.Query.Currency] = g.Aggregate(Money.Zero(request.Query.Currency), (acc, o) => acc + o.Amount)
            }))
            .ToList();
        
        return new StatisticOutcomesReadEvent()
        {
            SessionId = request.SessionId,
            
            DiagramTitle = $"Day by day expenses from {request.Query.MonthRange.From.ToString(DateFormat.FullMonthName)}",
            ChartPoints = chartPoints
        };
    }
}