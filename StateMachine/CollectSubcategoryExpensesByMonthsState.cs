using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectSubcategoryExpensesByMonthsState : IExpenseInfoState
{
    private readonly DateOnly _today;
    private readonly ILogger _logger;

    private readonly StateChain _chainState;
    private readonly ExpenseFilter _expenseFilter;
    private string DateFormat = "MMMM yyyy";

    public CollectSubcategoryExpensesByMonthsState(IEnumerable<Category> categories, DateOnly today, ILogger logger)
    {
        _today = today;
        _expenseFilter = new ExpenseFilter();
        
        _chainState = new StateChain(this,
            new CategorySubcategoryPicker(FilterUpdateStrategy<string>.FillCategory(_expenseFilter), FilterUpdateStrategy<string>.FillSubcategory(_expenseFilter), categories, logger),
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthFrom(_expenseFilter), "Enter the start period", _today, DateFormat,
            new[] { _today.AddYears(-1), _today.AddMonths(-6), _today.AddMonths(-1) }, "Another period"), 
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_expenseFilter)));
        
        _logger = logger;
    }

    public bool UserAnswerIsRequired => true;

    public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return _chainState.Request(botClient, chatId, cancellationToken);
    }

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        _chainState.Handle(message, cancellationToken);
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        return stateFactory.CreateChooseStatisticState();
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        var nextState = _chainState.ToNextState();

        if (nextState.IsOutOfChain)
        {
            var expenseAggregator = new ExpensesAggregator<DateOnly>(
                e => e.Date.LastDayOfMonth(), sortByMoney:false, sortAsc: true);

            return stateFactory.GetExpensesState(this, _expenseFilter,
                expenseAggregator,
                s => s.ToString(DateFormat),
                new TableOptions()
                {
                    Title = $"Category: {_expenseFilter.Category}. {Environment.NewLine}" +
                            $"Subcategory: {_expenseFilter.Subcategory}. {Environment.NewLine}" +
                            $"Expenses from {_expenseFilter.DateFrom.Value.ToString(DateFormat)}",
                    FirstColumnName = "Month"
                });
        }

        return this;
    }
}