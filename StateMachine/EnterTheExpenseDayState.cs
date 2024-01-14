using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class EnterTheExpenseDayState : IExpenseInfoState
    {
        public bool UserAnswerIsRequired => true;
        private readonly StateFactory _factory;
        protected readonly DateOnly _today;
        private readonly ILogger _logger;
        protected readonly string _dateFormat = "dd.MM.yyyy";
    
        public IExpenseInfoState PreviousState { get; private set; }

        public EnterTheExpenseDayState(StateFactory factory, IExpenseInfoState previousState, DateOnly today, ILogger logger)
        {
            _factory = factory;
            _today = today;
            _logger = logger;
            PreviousState = previousState;
        }
    
        public virtual async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            var info = "Enter the day";

            var yesterday = _today.AddDays(-1);

            var keyboard =
                TelegramKeyboard.FromButtons(
                    new TelegramButton[]
                    {
                        new()
                        {
                            Text = $"{_today.ToString("dd MMMM yyyy")}", 
                            CallbackData = _today.ToString(_dateFormat)
                        },
                        new()
                        {
                            Text = $"{yesterday.ToString("dd MMMM yyyy")}",
                            CallbackData = yesterday.ToString(_dateFormat)
                        },
                        new() { Text = "Another day", CallbackData = "custom" },
                    }, 3);
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: info,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public async Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Run(() => { });
        }

        public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
        {
            if (DateOnly.TryParseExact(message.Text, _dateFormat, out var selectedDay))
            {
                var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc:false);
                var specification = new ExpenseForTheDateSpecification(selectedDay);
                return _factory.GetExpensesState(this, specification,
                    expenseAggregator, 
                    s => s,
                    new TableOptions()
                    {
                        Title = selectedDay.ToString("dd MMMM yyyy"),
                        FirstColumnName = "Category"
                    });
            }
            
            if (message.Text == "custom")
            {
                return new EnterCustomExpenseDateState(_factory, this, _today, _logger);
            }

            return this;
        }
    }
    
    internal class EnterCustomExpenseDateState : EnterTheExpenseDayState
    {
        internal EnterCustomExpenseDateState(StateFactory factory, IExpenseInfoState previousState, DateOnly today, ILogger logger) : base(factory, previousState, today, logger)
        {
        }

        public override async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(chatId, $"Enter the day (for example, today is {_today.ToString(_dateFormat)})", cancellationToken: cancellationToken);
        }
    }
}