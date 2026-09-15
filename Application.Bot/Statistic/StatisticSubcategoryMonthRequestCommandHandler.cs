using Application.Core;
using Application.Core.Statistic;
using Domain;
using MediatR;

namespace Application.Bot.Statistic;

public class StatisticSubcategoryMonthRequestCommandHandler(IExpensesService expensesService, IMediator mediator) : StatisticQueryHandlerBase<StatisticSubcategoryMonthRequestCommand>(expensesService, mediator)
{
    protected override long GetSessionId(StatisticSubcategoryMonthRequestCommand request)
    {
        return request.SessionId;
    }

    protected override FinanceFilter BuildFilter(StatisticSubcategoryMonthRequestCommand request)
    {
        var query = request.Query;
        
        return new FinanceFilter()
        {
            DateFrom = query.MonthRange.From.ToDateOnly(),
            DateTo = query.MonthRange.To.ToDateOnly(),
            Category = query.Category,
            Subcategory = query.SubCategory,
            Currency = query.Currency,
        };
    }

    protected override INotification BuildReadEvent(StatisticSubcategoryMonthRequestCommand request, IReadOnlyList<Outcome> outcomes)
    {
        var currencies = outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
        
        var expenseAggregator = new ExpensesAggregator<DateOnly>(e => e.Date.FirstDayOfMonth(), false, sortAsc: true);
        var statistic = expenseAggregator.Aggregate(outcomes, currencies);
        
        
        var chartPoints = statistic.Rows
            .Select(r => (ChartBucket.ForMonth(YearMonth.From(r.Row)),
                (IReadOnlyDictionary<Currency, Money>)currencies.ToDictionary(c => c, c => r[c])))
            .ToList();
        
        return new StatisticOutcomesReadEvent()
        {
            SessionId = request.SessionId,
            
            Subtitle = $"Category: {request.Query.Category.Name}{Environment.NewLine}" +
                       $"Subcategory: {request.Query.SubCategory.Name}{Environment.NewLine}" +
                       $"Expenses from {request.Query.MonthRange.From.ToString(DateFormat.FullMonthName)}",
            FirstColumnName = "Month",
            
            Statistic = StatisticMapper.Map(statistic, new DateOnlyColumnFactory()),
            
            DiagramTitle = $"{request.Query.Category.Name} {request.Query.SubCategory.Name} expenses since {request.Query.MonthRange.From.ToString(DateFormat.FullMonthName)}",
            ChartPoints = chartPoints,
        };
    }
}