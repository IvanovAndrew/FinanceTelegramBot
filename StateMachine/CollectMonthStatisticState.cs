using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectMonthStatisticState : IExpenseInfoState
{
    private readonly DateOnly _today;
    private readonly ILogger _logger;
    private DatePickerState _datePicker; 
    private const string DateFormat = "MMMM yyyy";

    public CollectMonthStatisticState(DateOnly today, ILogger logger)
    {
        _today = today;
        
        _datePicker = new DatePickerState(this, "Enter the month", _today, DateFormat, 
            new[] { _today, _today.AddMonths(-1) }, "Another month");
        _logger = logger;
    }

    public bool UserAnswerIsRequired => true;
    
    public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return _datePicker.Request(botClient, chatId, cancellationToken);
    }

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) =>
        stateFactory.CreateChooseStatisticState();

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        var nextState = _datePicker.ToNextState(message, stateFactory, cancellationToken);

        if (nextState is DatePickerState datePickerState)
        {
            _datePicker = datePickerState;
            return this;
        }
        
        if (DateOnly.TryParseExact(message.Text, DateFormat, out var selectedMonth))
        {
            var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc:false);

            var expenseFilter =
                new ExpenseFilter()
                {
                    DateFrom = selectedMonth.FirstDayOfMonth(),
                    DateTo = selectedMonth.LastDayOfMonth()
                };
        
            return stateFactory.GetExpensesState(this, expenseFilter, expenseAggregator, s => s,
                new TableOptions(){Title = selectedMonth.ToString(DateFormat), FirstColumnName = "Category"});
        }

        return this;
    }
}