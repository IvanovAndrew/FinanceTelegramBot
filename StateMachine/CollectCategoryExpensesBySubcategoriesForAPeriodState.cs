using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectCategoryExpensesBySubcategoriesForAPeriodState : IExpenseInfoState
{
    private const string DateFormat = "MMMM yyyy";
    private readonly ExpenseFilter _expenseFilter;
    private readonly StateChain _stateChain;
    
    private readonly ILogger _logger;

    public CollectCategoryExpensesBySubcategoriesForAPeriodState(IEnumerable<Category> categories, DateOnly today, ILogger logger)
    {
        _logger = logger;
        
        _expenseFilter = new ExpenseFilter();
        _stateChain = new StateChain(this,
            new CategoryPicker(FilterUpdateStrategy<string>.FillCategory(_expenseFilter), categories, logger),
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

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) => stateFactory.CreateChooseStatisticState();

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        var moveStatus = _stateChain.ToNextState();
        if (moveStatus.IsOutOfChain)
        {
            var expenseAggregator = new ExpensesAggregator<string>(
                e => e.SubCategory ?? string.Empty, false, sortAsc: true);

            return stateFactory.GetExpensesState(this, _expenseFilter,
                expenseAggregator,
                s => s,
                new TableOptions()
                {
                    Title = $"Category: {_expenseFilter.Category}. {Environment.NewLine}" +
                            $"Expenses from {_expenseFilter.DateFrom.Value.ToString(DateFormat)}",
                    FirstColumnName = "Subcategory",
                });
            
        }

        return this;
    }
}