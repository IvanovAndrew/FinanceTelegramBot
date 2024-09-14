using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectCategoryExpensesState : IExpenseInfoState
{
    private readonly ILogger _logger;
    private readonly FinanseFilter _finanseFilter;
    private const string DateFormat = "MMMM yyyy";
    private readonly StateChain _stateChain;

    public CollectCategoryExpensesState(IEnumerable<Category> categories, DateOnly today, ILogger logger)
    {
        _logger = logger;
        _finanseFilter = new FinanseFilter();

        _stateChain = new StateChain(
            new CategoryPicker(FilterUpdateStrategy<string>.FillCategory(_finanseFilter), categories, _logger), 
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthFrom(_finanseFilter), "Enter the start period",
                today, DateFormat,
                new[] { today.AddYears(-1), today.AddMonths(-6), today.AddMonths(-1) }, "Another"),
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_finanseFilter)));
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
        stateFactory.CreateChooseStatisticState();

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        var moveStatus = _stateChain.ToNextState();

        if (moveStatus.IsOutOfChain)
        {
            var expenseAggregator = new ExpensesAggregator<DateOnly>(
                e => e.Date.LastDayOfMonth(), false, sortAsc: true);

            return stateFactory.GetExpensesState(this, _finanseFilter, expenseAggregator,
                s => s.ToString(DateFormat),
                new TableOptions()
                {
                    Title = $"Category: {_finanseFilter.Category}",
                    FirstColumnName = "Month"
                });
        }

        return this;
    }
}