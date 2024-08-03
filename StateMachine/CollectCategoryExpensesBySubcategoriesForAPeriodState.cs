using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectCategoryExpensesBySubcategoriesForAPeriodState : IExpenseInfoState
{
    private readonly Category _category;
    
    private const string DateFormat = "MMMM yyyy";
    private readonly ExpenseFilter _expenseFilter;
    private readonly StateChain _stateChain;
    
    private readonly ILogger _logger;

    public CollectCategoryExpensesBySubcategoriesForAPeriodState(Category category, DateOnly today, ILogger logger)
    {
        _category = category;
        _logger = logger;
        
        _expenseFilter = new ExpenseFilter(){Category = _category.Name};


        _stateChain = new StateChain(this,
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthFrom(_expenseFilter),
                "Choose start of the period", today, DateFormat,
                new[] { today.AddYears(-1), today.AddMonths(-6), today.AddMonths(-1) }, "Another"),
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_expenseFilter))
        );
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

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) => stateFactory.CreateEnterTypeOfCategoryStatistic(_category, this);

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        var nextState = _stateChain.ToNextState();
        if (nextState == this)
        {
            var expenseAggregator = new ExpensesAggregator<string>(
                e => e.SubCategory ?? string.Empty, false, sortAsc: true);

            return stateFactory.GetExpensesState(this, _expenseFilter,
                expenseAggregator,
                s => s,
                new TableOptions()
                {
                    Title = $"Category: {_category.Name}. {Environment.NewLine}" +
                            $"Expenses from {_expenseFilter.DateFrom.Value.ToString(DateFormat)}",
                    FirstColumnName = "Subcategory",
                });
            
        }

        return this;
    }
}