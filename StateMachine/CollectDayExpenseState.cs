using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class CollectDayExpenseState : IExpenseInfoState
    {
        public bool UserAnswerIsRequired => true;
        private readonly StateFactory _factory;
        private readonly DateOnly _today;
        private readonly ILogger _logger;
        private readonly string _dateFormat = "dd MMMM yyyy";
        private DatePickerState _datePicker;
    
        public IExpenseInfoState PreviousState { get; private set; }

        public CollectDayExpenseState(StateFactory factory, IExpenseInfoState previousState, DateOnly today, ILogger logger)
        {
            _factory = factory;
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

        public async Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Run(() => { });
        }

        public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
        {
            var nextState = _datePicker.ToNextState(message, cancellationToken);

            if (nextState is DatePickerState datePickerState)
            {
                _datePicker = datePickerState;
                return this;
            }
            
            if (DateOnly.TryParseExact(message.Text, _dateFormat, out var selectedDay))
            {
                var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc:false);
                var specification = new ExpenseForTheDateSpecification(selectedDay);
                return _factory.GetExpensesState(this, specification,
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