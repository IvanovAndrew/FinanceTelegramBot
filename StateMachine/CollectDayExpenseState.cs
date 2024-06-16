using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class CollectDayExpenseState : IExpenseInfoState
    {
        public bool UserAnswerIsRequired => true;
        private readonly DateOnly _today;
        private readonly ILogger _logger;
        private readonly string _dateFormat = "dd MMMM yyyy";
        private DatePickerState _datePicker;
    
        public IExpenseInfoState PreviousState { get; private set; }

        public CollectDayExpenseState(IExpenseInfoState previousState, DateOnly today, ILogger logger)
        {
            _today = today;
            _logger = logger;
            PreviousState = previousState;

            _datePicker = new DatePickerState(this, "Enter the day", today, _dateFormat,
                new[] { _today, _today.AddDays(-1) }, "Another day");
        }
    
        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
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

            if (nextState is DatePickerState datePickerState)
            {
                _datePicker = datePickerState;
                return this;
            }
            
            if (DateOnly.TryParseExact(message.Text, _dateFormat, out var selectedDay))
            {
                var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc:false);
                var specification = new ExpenseForTheDateSpecification(selectedDay);
                return stateFactory.GetExpensesState(this, specification,
                    expenseAggregator, 
                    s => s,
                    new TableOptions()
                    {
                        Title = selectedDay.ToString(_dateFormat),
                        FirstColumnName = "Category"
                    });
            }
            
            return this;
        }
    }
}