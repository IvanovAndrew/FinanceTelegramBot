using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectMonthStatisticState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly DateOnly _today;
    private readonly ILogger<StateFactory> _logger;
    private DatePickerState _datePicker; 
    private const string DateFormat = "MMMM yyyy";

    public IExpenseInfoState PreviousState { get; }
    
    public CollectMonthStatisticState(StateFactory factory, IExpenseInfoState previousState, DateOnly today, ILogger<StateFactory> logger)
    {
        _factory = factory;
        PreviousState = previousState;
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

    public Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        return Task.Run(() => { });
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
    {
        var nextState = _datePicker.ToNextState(message, cancellationToken);

        if (nextState is DatePickerState datePickerState)
        {
            _datePicker = datePickerState;
            return this;
        }
        
        if (DateOnly.TryParseExact(message.Text, DateFormat, out var selectedMonth))
        {
            var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc:false);

            var specification =
                new ExpenseFromDateRangeSpecification(selectedMonth.FirstDayOfMonth(), selectedMonth.LastDayOfMonth());
        
            return _factory.GetExpensesState(this, specification, expenseAggregator, s => s,
                new TableOptions(){Title = selectedMonth.ToString(DateFormat), FirstColumnName = "Category"});
        }

        return this;
    }
}