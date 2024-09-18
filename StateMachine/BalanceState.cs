using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class BalanceState : StateWithChainsBase
{
    private readonly FinanceFilter _filter = new(){Income = true};
    private string DateFormat = "MMMM yyyy";
    internal BalanceState(DateOnly today, ILogger logger)
    {
        StateChain = new StateChain(
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthFrom(_filter), "Choose the start of the period", today, DateFormat, new DateOnly[] { today.AddMonths(-6), today.AddMonths(-1), today }.ToDictionary(d => d, d => d.ToString(DateFormat)), "Another", logger),
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_filter)));
    }
    
    protected override IExpenseInfoState PreviousState(IStateFactory stateFactory)
    {
        return stateFactory.CreateChooseStatisticState();
    }

    protected override IExpenseInfoState NextState(IStateFactory stateFactory)
    {
        return stateFactory.CreateBalanceStatisticState(_filter);
    }
}