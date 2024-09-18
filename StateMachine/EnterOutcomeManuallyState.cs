using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class EnterOutcomeManuallyState : StateWithChainsBase
{
    private readonly MoneyTransferBuilder _moneyTransferBuilder;
    
    internal EnterOutcomeManuallyState(DateOnly today, IEnumerable<Category> categories, ILogger logger)
    {
        _moneyTransferBuilder = new MoneyTransferBuilder(false);
        StateChain = new StateChain
        (
            new DatePickerState(new UpdateOutcomeDateStrategy(_moneyTransferBuilder), "Enter the date", today, "dd.MM.yyyy", new Dictionary<DateOnly, string>(){[today] = "Today", [today.AddDays(-1)] = "Yesterday"}, "Another day", logger),
            new CategorySubcategoryPicker(c => _moneyTransferBuilder.Category = c, sc => _moneyTransferBuilder.SubCategory = sc, categories, logger),
            new DescriptionPicker(d => _moneyTransferBuilder.Description = d),
            new AmountPicker(m => _moneyTransferBuilder.Sum = m, "price")
        );
    }
    
    protected override IExpenseInfoState PreviousState(IStateFactory stateFactory)
    {
        return stateFactory.WayOfEnteringExpenseState();
    }

    protected override IExpenseInfoState NextState(IStateFactory stateFactory)
    {
        return stateFactory.CreateConfirmState(_moneyTransferBuilder.Build());
    }
}