using Application.Core;
using Domain;
using Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Bot.Statistic;

public record GetStatisticCategoryRequestCommand : IRequest
{
    public long SessionId { get; init; }
    public required StatisticsQuery Query { get; init; }
}

public class GetStatisticCategoryRequestCommandHandler(IExpensesService expensesService, IMediator mediator, ILogger<GetStatisticCategoryRequestCommandHandler> logger)
    : StatisticQueryHandlerBase<GetStatisticCategoryRequestCommand>(expensesService, mediator)
{
    protected override long GetSessionId(GetStatisticCategoryRequestCommand r) => r.SessionId;

    protected override FinanceFilter BuildFilter(GetStatisticCategoryRequestCommand r) => new()
    {
        DateFrom = r.Query.MonthRange.From.ToDateOnly(), 
        Category = r.Query.Category, 
        Currency = r.Query.Currency
    };

    protected override INotification BuildReadEvent(GetStatisticCategoryRequestCommand r,
        IReadOnlyList<Outcome> outcomes)
    {
        logger.LogInformation($"{nameof(GetStatisticCategoryRequestCommandHandler)}.{nameof(BuildReadEvent)}: {r} {string.Join(", ", outcomes.Select(c => c.Category).Distinct())}");
        
        var currencies = outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
        
        var expenseAggregator = new ExpensesAggregator<YearMonth>(e => YearMonth.From(e.Date), sortByMoney: false, sortAsc: true);
        var statistic = expenseAggregator.Aggregate(outcomes, outcomes.Select(c => c.Amount.Currency).Distinct().ToArray());
        var wrapper = StatisticMapper.Map(statistic, new MonthColumnFactory());
        
        logger.LogInformation($"{nameof(GetStatisticCategoryRequestCommandHandler)}.{nameof(BuildReadEvent)} finishing");
        
        return new StatisticOutcomesReadEvent()
        {
            SessionId = r.SessionId,

            Subtitle = $"Category: {r.Query.Category.Name}{Environment.NewLine}Start Date Range:{r.Query.MonthRange.From.ToString(DateFormat.FullMonthName)}",
            FirstColumnName = "Month",
            Statistic = wrapper,
            
            DiagramTitle = $"Category: {r.Query.Category.Name}",
            ChartPoints = statistic.Rows
                    .Select(r => (ChartBucket.ForMonth(r.Row),
                    (IReadOnlyDictionary<Currency, Money>)currencies.ToDictionary(c => c, c => r[c])))
                        .ToList()
        };
    }
}