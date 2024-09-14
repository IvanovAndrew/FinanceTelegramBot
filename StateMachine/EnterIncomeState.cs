using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class EnterIncomeState : IExpenseInfoState
{
    private readonly ILogger<StateFactory> _logger;
    private readonly StateChain _stateChain;
    private readonly Income _income;

    public EnterIncomeState(DateOnly today, IEnumerable<IncomeCategory> incomeCategories, ILogger<StateFactory> logger)
    {
        _income = new Income();
        _stateChain = new StateChain(
            new DatePickerState(new UpdateIncomeDateStrategy(_income), "Enter the date", today, "dd.MM.yyyy", new []{today, today.AddDays(-1)}, "Another date"),
            new IncomeCategoryPicker(category => _income.Category = category.Name, incomeCategories),
            new IncomeDescription(description => _income.Description = description),
            new AmountPicker(amount => _income.Amount = amount)
        );
        _logger = logger;
    }

    public bool UserAnswerIsRequired => true;
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await _stateChain.Request(botClient, chatId, cancellationToken);
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        var moveStatus = _stateChain.MoveToPreviousState();
        return moveStatus.IsOutOfChain? stateFactory.CreateGreetingState() : this;
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory, CancellationToken cancellationToken)
    {
        var moveStatus = _stateChain.ToNextState();
        return moveStatus.IsOutOfChain? stateFactory.CreateConfirmState(_income) : this;
    }

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        _stateChain.Handle(message, cancellationToken);
        return Task.CompletedTask;
    }
}