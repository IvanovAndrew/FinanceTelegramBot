using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class CollectDayExpenseState : StateWithChainsBase
    {
        private readonly DateOnly _today;
        
        private readonly string _dateFormat = "dd MMMM yyyy";
        private readonly FinanceFilter _financeFilter;
        private readonly ILogger _logger;

        public CollectDayExpenseState(DateOnly today, ILogger logger)
        {
            _today = today;
            _financeFilter = new FinanceFilter();

            StateChain = new StateChain(
                new DatePickerState(FilterUpdateStrategy<DateOnly>.FillDate(_financeFilter), "Enter the day", today, _dateFormat,
                new Dictionary<DateOnly, string>() { [_today] = "Today", [_today.AddDays(-1)] = "Yesterday" }, "Another day", logger), 
                new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_financeFilter)));
            
            _logger = logger;
        }
        
        protected override IExpenseInfoState PreviousState(IStateFactory stateFactory) =>
            stateFactory.CreateChooseStatisticState();
    
        protected override IExpenseInfoState NextState(IStateFactory stateFactory)
        {
            var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc: false);

            return stateFactory.GetExpensesState(this, _financeFilter,
                expenseAggregator,
                s => s,
                new TableOptions()
                {
                    Title = _financeFilter.DateTo!.Value.ToString(_dateFormat),
                    FirstColumnName = "Category"
                });
        }
    }
}