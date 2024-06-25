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

    private IExpenseInfoState _datePicker;
    private string DateFormat = "MMMM yyyy";

    public CollectSubCategoryExpensesByMonthsState(DateOnly today, Category category, SubCategory subCategory, ILogger logger)
    {
        _today = today;
        _category = category;
        _subCategory = subCategory;
        _logger = logger;
        
        _datePicker = new DatePickerState(this, "Enter the start period", _today, DateFormat,
            new[] { _today.AddYears(-1), _today.AddMonths(-6), _today.AddMonths(-1) }, "Another period");
    }

    public bool UserAnswerIsRequired => true;

    public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return _datePicker.Request(botClient, chatId, cancellationToken);
    }

    public Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        return stateFactory.EnterSubcategoryStatisticState(this, _category);
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        var nextState = _datePicker.MoveToNextState(message, stateFactory, cancellationToken);

        if (nextState is DatePickerState datePicker)
        {
            _datePicker = datePicker;
            return this;
        }

        if (DateOnly.TryParseExact(message.Text, DateFormat, out var dateFrom))
        {
            var firstDayOfMonth = dateFrom.FirstDayOfMonth();
            var expenseAggregator = new ExpensesAggregator<DateOnly>(
                e => e.Date.LastDayOfMonth(), true, sortAsc: false);

            var specification = new MultipleSpecification(
                new ExpenseLaterThanSpecification(firstDayOfMonth),
                new ExpenseFromCategoryAndSubcategorySpecification(_category, _subCategory));

            return stateFactory.GetExpensesState(this, specification,
                expenseAggregator,
                s => s.ToString(DateFormat),
                new TableOptions()
                {
                    Title = $"Category: {_category.Name}. {Environment.NewLine}" +
                            $"Subcategory: {_subCategory.Name}. {Environment.NewLine}" +
                            $"Expenses from {firstDayOfMonth.ToString(DateFormat)}",
                    FirstColumnName = "Month"
                });
        }

        return this;
    }
}