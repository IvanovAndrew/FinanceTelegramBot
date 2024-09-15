using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectMonthStatisticState : StateWithChainsBase
{
    private readonly DateOnly _today;
    
    private readonly FinanceFilter _financeFilter;
    private const string DateFormat = "MMMM yyyy";
    private readonly ILogger _logger;

    public CollectMonthStatisticState(DateOnly today, ILogger logger)
    {
        _today = today;
        _financeFilter = new FinanceFilter();
        
        StateChain = new StateChain( 
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthRange(_financeFilter), "Enter the month", _today, DateFormat, 
                new Dictionary<DateOnly, string>
                {
                    [_today] = "This month",
                    [_today.AddMonths(-1)] = "Previous month",
                }, 
                "Another month", 
                logger), 
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_financeFilter))); 
        _logger = logger;
    }

    protected override IExpenseInfoState PreviousState(IStateFactory stateFactory)
    {
        return stateFactory.CreateChooseStatisticState();
    }

    protected override IExpenseInfoState NextState(IStateFactory stateFactory)
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc:false);

        return stateFactory.GetExpensesState(this, _financeFilter, expenseAggregator, s => s,
            new TableOptions(){Title = _financeFilter.DateTo.Value.ToString(DateFormat), FirstColumnName = "Category"});
    }
}