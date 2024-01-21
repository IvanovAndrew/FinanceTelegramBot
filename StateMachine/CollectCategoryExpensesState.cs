using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectCategoryExpensesState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly Category _category;
    private readonly ILogger<StateFactory> _logger;
    private IExpenseInfoState _datePicker;
    private const string DateFormat = "MMMM yyyy";

    public CollectCategoryExpensesState(StateFactory factory, IExpenseInfoState previousState, DateOnly today, Category category, ILogger<StateFactory> logger)
    {
        _factory = factory;
        _category = category;
        _logger = logger;
        PreviousState = previousState;
        _datePicker = new DatePickerState(this, "Enter the start period", today, DateFormat,
            new[] { today.AddYears(-1), today.AddMonths(-6), today.AddMonths(-1) }, "Another");
    }

    public bool UserAnswerIsRequired => true;
    public IExpenseInfoState PreviousState { get; }
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await _datePicker.Request(botClient, chatId, cancellationToken);
    }

    public Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
    {
        var nextState = _datePicker.ToNextState(message, cancellationToken);

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


            return _factory.GetExpensesState(this, specification, expenseAggregator,
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