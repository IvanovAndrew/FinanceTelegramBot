using Infrastructure;
using Microsoft.Extensions.Logging;
using TelegramBot;

namespace StateMachine
{
    class GreetingState : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly ILogger _logger;
    
        public IExpenseInfoState PreviousState { get; private set; }
    
        public GreetingState(StateFactory factory, ILogger logger)
        {
            _factory = factory;
            _logger = logger;
            PreviousState = this;
        }

        public bool UserAnswerIsRequired => true;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            var keyboard = 
                TelegramKeyboard.FromButtons(new[]
                {
                    new TelegramButton() { Text = "Outcome", CallbackData = "startExpense" },
                    new TelegramButton() { Text = "Statistics", CallbackData = "showExpenses" },
                });

            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "What would you like to do?",
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            return text == "showExpenses" ? 
                _factory.CreateChooseStatisticState(this) : 
                _factory.CreateEnterTheDateState(this);
        }
    }
}