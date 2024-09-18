using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine.Statistics;

internal class CollectCategoryExpensesBySubcategoriesForAPeriodState : StateWithChainsBase
{
    private const string DateFormat = "MMMM yyyy";
    private readonly FinanceFilter _financeFilter;
    
    private readonly ILogger _logger;

    public CollectCategoryExpensesBySubcategoriesForAPeriodState(IEnumerable<Category> categories, DateOnly today, ILogger logger)
    {
        _logger = logger;
        
        _financeFilter = new FinanceFilter();
        StateChain = new StateChain(
            new CategoryPicker(c => _financeFilter.Category = c.Name, categories, logger),
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthFrom(_financeFilter),
                "Choose start of the period", today, DateFormat,
                new[] { today.AddYears(-1), today.AddMonths(-6), today.AddMonths(-1) }.ToDictionary(d => d, d => d.ToString(DateFormat)), "Another", logger),
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_financeFilter))
        );
    }

    protected override IExpenseInfoState NextState(IStateFactory stateFactory) 
    {
        var expenseAggregator = new ExpensesAggregator<string>(
            e => e.SubCategory ?? string.Empty, false, sortAsc: true);

        return stateFactory.GetExpensesState(this, _financeFilter,
            expenseAggregator,
            s => s,
            new TableOptions()
            {
                Title = $"Category: {_financeFilter.Category}. {Environment.NewLine}" +
                        $"Expenses from {_financeFilter.DateFrom.Value.ToString(DateFormat)}",
                FirstColumnName = "Subcategory",
            });
    }
    
    protected override IExpenseInfoState PreviousState(IStateFactory stateFactory) =>
        stateFactory.CreateChooseStatisticState();
}