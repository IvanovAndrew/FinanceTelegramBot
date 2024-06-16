using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectCategoryExpensesBySubcategoriesForAPeriodState : IExpenseInfoState
{
    private readonly Category _category;
    private readonly ILogger _logger;
    private DatePickerState _datePicker;
    private const string DateFormat = "MMMM yyyy";

    public CollectCategoryExpensesBySubcategoriesForAPeriodState(IExpenseInfoState previousState, Category category, DateOnly today, ILogger logger)
    {
        PreviousState = previousState;
        _category = category;
        _logger = logger;
        _datePicker = new DatePickerState(this, "Choose start of the period", today, DateFormat,
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

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        var nextState = _datePicker.ToNextState(message, stateFactory, cancellationToken);

        if (nextState is DatePickerState datePicker)
        {
            _datePicker = datePicker;
            return this;
        }

        if (DateOnly.TryParseExact(message.Text, DateFormat, out var dateFrom))
        {
            var firstDayOfMonth = dateFrom.FirstDayOfMonth();
            var expenseAggregator = new ExpensesAggregator<string>(
                e => e.SubCategory ?? string.Empty, true, sortAsc: false);

            var specification = new MultipleSpecification(
                new ExpenseLaterThanSpecification(firstDayOfMonth),
                new ExpenseFromCategorySpecification(_category));
            
            return stateFactory.GetExpensesState(this, specification,
                expenseAggregator,
                s => s,
                new TableOptions()
                {
                    Title = $"Category: {_category.Name}. {Environment.NewLine}" +
                            $"Expenses from {firstDayOfMonth.ToString(DateFormat)}",
                    FirstColumnName = "Subcategory",
                });
            
        }

        return this;
    }
}