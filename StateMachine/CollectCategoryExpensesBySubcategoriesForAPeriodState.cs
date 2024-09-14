using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectCategoryExpensesBySubcategoriesForAPeriodState : IExpenseInfoState
{
    private const string DateFormat = "MMMM yyyy";
    private readonly FinanseFilter _finanseFilter;
    private readonly StateChain _stateChain;
    
    private readonly ILogger _logger;

    public CollectCategoryExpensesBySubcategoriesForAPeriodState(IEnumerable<Category> categories, DateOnly today, ILogger logger)
    {
        _logger = logger;
        
        _finanseFilter = new FinanseFilter();
        _stateChain = new StateChain(
            new CategoryPicker(FilterUpdateStrategy<string>.FillCategory(_finanseFilter), categories, logger),
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthFrom(_finanseFilter),
                "Choose start of the period", today, DateFormat,
                new[] { today.AddYears(-1), today.AddMonths(-6), today.AddMonths(-1) }, "Another"),
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_finanseFilter))
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

            return stateFactory.GetExpensesState(this, _finanseFilter,
                expenseAggregator,
                s => s,
                new TableOptions()
                {
                    Title = $"Category: {_finanseFilter.Category}. {Environment.NewLine}" +
                            $"Expenses from {_finanseFilter.DateFrom.Value.ToString(DateFormat)}",
                    FirstColumnName = "Subcategory",
                });
            
        }

        return this;
    }
}