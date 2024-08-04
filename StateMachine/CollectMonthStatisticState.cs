using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectMonthStatisticState : IExpenseInfoState
{
    private readonly DateOnly _today;
    
    private readonly StateChain _stateChain;
    private readonly ExpenseFilter _expenseFilter;
    private const string DateFormat = "MMMM yyyy";
    private readonly ILogger _logger;

    public CollectMonthStatisticState(DateOnly today, ILogger logger)
    {
        _today = today;
        _expenseFilter = new ExpenseFilter();
        
        _stateChain = new StateChain(this, 
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthRange(_expenseFilter), "Enter the month", _today, DateFormat, 
                new[] { _today, _today.AddMonths(-1) }, "Another month"), 
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_expenseFilter))); 
        _logger = logger;
    }

    public bool UserAnswerIsRequired => true;
    
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await _stateChain.Request(botClient, chatId, cancellationToken);
    }

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        _stateChain.Handle(message, cancellationToken);
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        var previousState = _stateChain.MoveToPreviousState();
        return previousState.IsOutOfChain ? stateFactory.CreateChooseStatisticState() : this;
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        var nextState = _stateChain.ToNextState();

        if (nextState.IsOutOfChain)
        {
            var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc:false);

            return stateFactory.GetExpensesState(this, _expenseFilter, expenseAggregator, s => s,
                new TableOptions(){Title = _expenseFilter.DateTo.Value.ToString(DateFormat), FirstColumnName = "Category"});
        }

        return this;
    }
}