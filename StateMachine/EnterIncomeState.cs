using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class EnterIncomeState : StateWithChainsBase
{
    private readonly Income _income;
    private readonly ILogger<StateFactory> _logger;

    public EnterIncomeState(DateOnly today, IEnumerable<IncomeCategory> incomeCategories, ILogger<StateFactory> logger)
    {
        _income = new Income();
        StateChain = new StateChain(
            new DatePickerState(new UpdateIncomeDateStrategy(_income), "Enter the date", today, "dd.MM.yyyy", new []{today, today.AddDays(-1)}, "Another date"),
            new IncomeCategoryPicker(category => _income.Category = category.Name, incomeCategories),
            new DescriptionPicker(description => _income.Description = description),
            new AmountPicker(amount => _income.Amount = amount)
        );
        _logger = logger;
    }

    protected override IExpenseInfoState PreviousState(IStateFactory stateFactory)
    {
        return stateFactory.CreateGreetingState();
    }

    protected override IExpenseInfoState NextState(IStateFactory stateFactory)
    {
        return stateFactory.CreateConfirmState(_income);
    }
}