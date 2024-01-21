using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectSubCategoryExpensesState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly DateOnly _today;
    private readonly Category _category;
    private readonly SubCategory _subCategory;
    private readonly ILogger<StateFactory> _logger;

    private IExpenseInfoState _datePicker;
    private string DateFormat = "MMM yyyy";

    public CollectSubCategoryExpensesState(StateFactory factory, IExpenseInfoState previousState, DateOnly today, Category category, SubCategory subCategory, ILogger<StateFactory> logger)
    {
        _factory = factory;
        PreviousState = previousState;
        _today = today;
        _category = category;
        _subCategory = subCategory;
        _logger = logger;
        _datePicker = new DatePickerState(previousState, "Enter the start period", _today, DateFormat,
            new[] { _today.AddYears(-1), _today.AddMonths(-6), _today.AddMonths(-1) }, "Another period");
    }

    public bool UserAnswerIsRequired { get; }
    public IExpenseInfoState PreviousState { get; }
    public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return _datePicker.Request(botClient, chatId, cancellationToken);
    }

    public Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
    {
        var nextState = _datePicker.ToNextState(message, cancellationToken);

        if (nextState is DatePickerState datePicker)
        {
            _datePicker = datePicker;
            return _datePicker;
        }

        if (DateOnly.TryParseExact(message.Text, DateFormat, out var dateFrom))
        {
            var firstDayOfMonth = dateFrom.FirstDayOfMonth();
            var expenseAggregator = new ExpensesAggregator<string>(
                e => e.SubCategory ?? string.Empty, true, sortAsc: false);

            var specification = new MultipleSpecification(
                new ExpenseLaterThanSpecification(firstDayOfMonth),
                new ExpenseFromCategorySpecification(_category));
            
            return _factory.GetExpensesState(this, specification,
                expenseAggregator,
                s => s,
                new TableOptions()
                {
                    Title = $"Category: {_category}. {Environment.NewLine}" +
                            $"Expenses from {firstDayOfMonth.ToString(DateFormat)}",
                    FirstColumnName = "Subcategory"
                });
            
        }

        return this;
    }
}