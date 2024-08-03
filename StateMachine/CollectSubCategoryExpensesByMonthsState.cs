using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectSubCategoryExpensesByMonthsState : IExpenseInfoState
{
    private readonly Category _category;
    private readonly SubCategory _subCategory;
    private readonly DateOnly _today;
    private readonly ILogger _logger;

    private readonly StateChain _chainState;
    private readonly ExpenseFilter _expenseFilter;
    private string DateFormat = "MMMM yyyy";

    public CollectSubCategoryExpensesByMonthsState(DateOnly today, Category category, SubCategory subCategory, ILogger logger)
    {
        _today = today;
        _category = category;
        _subCategory = subCategory;

        _expenseFilter = new ExpenseFilter() { Category = _category.Name, Subcategory = _subCategory.Name };
        
        _chainState = new StateChain(this, 
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
        return stateFactory.EnterSubcategoryStatisticState(this, _category);
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        var nextState = _chainState.ToNextState();

        if (nextState == this)
        {
            var expenseAggregator = new ExpensesAggregator<DateOnly>(
                e => e.Date.LastDayOfMonth(), true, sortAsc: false);

            return stateFactory.GetExpensesState(this, _expenseFilter,
                expenseAggregator,
                s => s.ToString(DateFormat),
                new TableOptions()
                {
                    Title = $"Category: {_category.Name}. {Environment.NewLine}" +
                            $"Subcategory: {_subCategory.Name}. {Environment.NewLine}" +
                            $"Expenses from {_expenseFilter.DateFrom.Value.ToString(DateFormat)}",
                    FirstColumnName = "Month"
                });
        }

        return this;
    }
}