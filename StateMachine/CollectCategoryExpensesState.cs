using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectCategoryExpensesState : IExpenseInfoState
{
    private readonly Category _category;
    private readonly ILogger _logger;
    private IExpenseInfoState _datePicker;
    private const string DateFormat = "MMMM yyyy";

    public CollectCategoryExpensesState(DateOnly today, Category category, ILogger logger)
    {
        _category = category;
        _logger = logger;
        _datePicker = new DatePickerState(this, "Enter the start period", today, DateFormat,
            new[] { today.AddYears(-1), today.AddMonths(-6), today.AddMonths(-1) }, "Another");
    }

    public bool UserAnswerIsRequired => true;
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await _datePicker.Request(botClient, chatId, cancellationToken);
    }

    public Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) =>
        stateFactory.CreateEnterTypeOfCategoryStatistic(_category, this);

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        var nextState = _datePicker.MoveToNextState(message, stateFactory, cancellationToken);

        if (nextState is DatePickerState datePickerState)
        {
            _datePicker = datePickerState;
            return this;
        }

        if (DateOnly.TryParseExact(message.Text, DateFormat, out var monthFrom))
        {
            var expenseAggregator = new ExpensesAggregator<DateOnly>(
                e => e.Date.LastDayOfMonth(), false, sortAsc: true);

            var specification =
                new MultipleSpecification(
                    new ExpenseLaterThanSpecification(monthFrom),
                    new ExpenseFromCategorySpecification(_category)
                );


            return stateFactory.GetExpensesState(this, specification, expenseAggregator,
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