using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectCategoryExpensesState : IExpenseInfoState
{
    private readonly Category _category;
    private readonly ILogger _logger;
    private readonly ExpenseFilter _expenseFilter;
    private const string DateFormat = "MMMM yyyy";
    private readonly StateChain _stateChain;

    public CollectCategoryExpensesState(DateOnly today, Category category, ILogger logger)
    {
        _category = category;
        _logger = logger;
        _expenseFilter = new ExpenseFilter(){Category = _category.Name};

        _stateChain = new StateChain(this,
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthFrom(_expenseFilter), "Enter the start period",
                today, DateFormat,
                new[] { today.AddYears(-1), today.AddMonths(-6), today.AddMonths(-1) }, "Another"),
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_expenseFilter)));
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

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) =>
        stateFactory.CreateEnterTypeOfCategoryStatistic(_category, this);

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        var nextState = _stateChain.ToNextState();

        if (nextState == this)
        {
            var expenseAggregator = new ExpensesAggregator<DateOnly>(
                e => e.Date.LastDayOfMonth(), false, sortAsc: true);

            return stateFactory.GetExpensesState(this, _expenseFilter, expenseAggregator,
                s => s.ToString(DateFormat),
                new TableOptions()
                {
                    Title = $"Category: {_category.Name}",
                    FirstColumnName = "Month"
                });
        }

        return this;
    }
}