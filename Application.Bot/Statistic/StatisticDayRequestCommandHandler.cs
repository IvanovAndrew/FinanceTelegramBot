using Application.Core;
using Application.Core.Statistic;
using Domain;
using MediatR;

namespace Application.Bot.Statistic;

public class StatisticDayRequestCommandHandler(IFinanceRepository repo, IMediator mediator)
    : StatisticQueryHandlerBase<StatisticDayRequestCommand>(repo, mediator)
{
    protected override long GetSessionId(StatisticDayRequestCommand r) => r.SessionId;

    protected override FinanceFilter BuildFilter(StatisticDayRequestCommand r) => new()
    {
        DateFrom = r.Query.Period.From, DateTo = r.Query.Period.From, Currency = r.Query.Currency
    };

    protected override INotification BuildReadEvent(StatisticDayRequestCommand r, IReadOnlyList<Outcome> outcomes)
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category.Name, sortByMoney: true, sortAsc: false);
        var statistic = expenseAggregator.Aggregate(outcomes, outcomes.Select(c => c.Amount.Currency).Distinct().ToArray());
        var wrapper = StatisticMapper.Map(statistic, new StringColumnFactory());
        
        return new StatisticOutcomesReadEvent
        {
            SessionId = r.SessionId,
            Statistic = wrapper,
            Subtitle = $"Expenses for {r.Query.Period.From.ToString(DateFormat.Day)}", 
            FirstColumnName = "Category", 
        };
    }
}