using Application.Bot.Flows;
using Application.Core;
using Domain;
using Domain.Services;
using MediatR;

namespace Application.Bot.Statistic;

public class StatisticSubcategoryRequestCommand : IRequest
{
    public long SessionId { get; init; }
    public StatisticsQuery Query { get; init; }
}

public class StatisticSubcategoryRequestCommandHandler(IExpensesService expensesService, IMediator mediator) : StatisticQueryHandlerBase<StatisticSubcategoryRequestCommand>(expensesService, mediator)
{
    protected override long GetSessionId(StatisticSubcategoryRequestCommand request)
    {
        return request.SessionId;
    }

    protected override FinanceFilter BuildFilter(StatisticSubcategoryRequestCommand request)
    {
        return new FinanceFilter()
        {
            DateFrom = request.Query.MonthRange.From.ToDateOnly(),
            Category = request.Query.Category,
            Subcategory = request.Query.SubCategory,
            Currency = request.Query.Currency,
        };
    }

    protected override INotification BuildReadEvent(StatisticSubcategoryRequestCommand request, IReadOnlyList<Outcome> outcomes)
    {
        var currencies = outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
            
        var expenseAggregator = new ExpensesAggregator<string>(e => e.SubCategory?.Name ?? "<Unknown>", true, sortAsc: false);
        var aggregation = expenseAggregator.Aggregate(outcomes, currencies);

        var statistic = StatisticMapper.Map(aggregation, new StringColumnFactory());
        
        return new StatisticOutcomesReadEvent()
        {
            SessionId = request.SessionId,

            Subtitle = $"Category: {request.Query.Category.Name}{Environment.NewLine}Expenses from {request.Query.MonthRange.From.ToString(DateFormat.FullMonthName)}",
            FirstColumnName = "Subcategory",

            Statistic = statistic,
        };
    }
}