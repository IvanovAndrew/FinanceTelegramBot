using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectMonthStatisticState : IExpenseInfoState
{
    private readonly DateOnly _today;
    
    private readonly StateChain _stateChain;
    private readonly FinanseFilter _finanseFilter;
    private const string DateFormat = "MMMM yyyy";
    private readonly ILogger _logger;

    public CollectMonthStatisticState(DateOnly today, ILogger logger)
    {
        _today = today;
        _finanseFilter = new FinanseFilter();
        
        _stateChain = new StateChain( 
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthRange(_finanseFilter), "Enter the month", _today, DateFormat, 
                new[] { _today, _today.AddMonths(-1) }, "Another month"), 
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_finanseFilter))); 
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

            return stateFactory.GetExpensesState(this, _finanseFilter, expenseAggregator, s => s,
                new TableOptions(){Title = _finanseFilter.DateTo.Value.ToString(DateFormat), FirstColumnName = "Category"});
        }

        return this;
    }
}