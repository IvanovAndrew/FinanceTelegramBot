using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class EnterTheExpenseDayState : IExpenseInfoState
    {
        public bool UserAnswerIsRequired => true;
        private readonly StateFactory _factory;
        private readonly DateOnly _today;
        private readonly ILogger _logger;
        private readonly string _dateFormat = "yyyy-MM-dd";
    
        public IExpenseInfoState PreviousState { get; private set; }

        public EnterTheExpenseDayState(StateFactory factory, IExpenseInfoState previousState, DateOnly today, ILogger logger)
        {
            _factory = factory;
            _today = today;
            _logger = logger;
            PreviousState = previousState;
        }
    
        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            var info = "Enter the day";
            
            var keyboard = 
                TelegramKeyboard.FromButtons(
                    Enumerable.Range(0, 6).Reverse().Select(c => _today.AddDays(-c))
                .Select(date => new TelegramButton() {Text = $"{date.ToString("dd MMMM yyyy")}", CallbackData = date.ToString(_dateFormat)}), 3);
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: info,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            if (DateOnly.TryParseExact(text, _dateFormat, out var selectedDay))
            {
                var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc:false);
                var specification = new ExpenseForTheDateSpecification(selectedDay);
                return _factory.GetExpensesState(this, specification,
                    expenseAggregator, 
                    s => s,
                    new TableOptions()
                    {
                        Title = selectedDay.ToString("dd MMMM yyyy"),
                        ColumnNames = new []{"Category", "AMD", "RUR"}
                    });
            }

            return this;
        }
    }
}