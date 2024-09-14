using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class EnterOutcomeManuallyState : StateWithChainsBase
{
    private readonly ExpenseBuilder _expenseBuilder;
    
    internal EnterOutcomeManuallyState(DateOnly today, IEnumerable<Category> categories, ILogger logger)
    {
        _expenseBuilder = new ExpenseBuilder();
        StateChain = new StateChain
        (
            new DatePickerState(new UpdateOutcomeDateStrategy(_expenseBuilder), "Enter the date", today, "dd.MM.yyyy", new Dictionary<DateOnly, string>(){[today] = "Today", [today.AddDays(-1)] = "Yesterday"}, "Another day", logger),
            new CategorySubcategoryPicker(c => _expenseBuilder.Category = c, sc => _expenseBuilder.SubCategory = sc, categories, logger),
            new DescriptionPicker(d => _expenseBuilder.Description = d),
            new AmountPicker(m => _expenseBuilder.Sum = m, "price")
        );
    }
    
    protected override IExpenseInfoState PreviousState(IStateFactory stateFactory)
    {
        return stateFactory.WayOfEnteringExpenseState();
    }

    protected override IExpenseInfoState NextState(IStateFactory stateFactory)
    {
        return stateFactory.CreateConfirmState(_expenseBuilder.Build());
    }
}