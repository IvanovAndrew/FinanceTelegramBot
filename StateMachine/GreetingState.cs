using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class GreetingState : IExpenseInfoState
    {
        private readonly ILogger _logger;
    
        public GreetingState(ILogger logger)
        {
            _logger = logger;
        }

        public bool UserAnswerIsRequired => true;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            var keyboard = 
                TelegramKeyboard.FromButtons(new[]
                {
                    new TelegramButton() { Text = "Outcome", CallbackData = "startExpense" },
                    new TelegramButton() { Text = "Income", CallbackData = "startIncome" },
                    new TelegramButton() { Text = "Statistics", CallbackData = "showExpenses" },
                });

            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "What would you like to do?",
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            return this;
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            if (message.Text == "showExpenses") return stateFactory.CreateChooseStatisticState(); 
            if (message.Text == "startExpense") return stateFactory.WayOfEnteringExpenseState();
            if (message.Text == "startIncome") return stateFactory.CreateEnterIncomeState();
            
            var start = stateFactory.WayOfEnteringExpenseState();
            return start.MoveToNextState(message, stateFactory, cancellationToken);
        }
    }
}