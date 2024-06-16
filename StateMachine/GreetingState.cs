using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class GreetingState : IExpenseInfoState
    {
        private readonly ILogger _logger;
    
        public IExpenseInfoState PreviousState { get; private set; }
    
        public GreetingState(ILogger logger)
        {
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

        public Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            if (message.Text == "showExpenses") return stateFactory.CreateChooseStatisticState(this); 
            if (message.Text == "startExpense") return stateFactory.WayOfEnteringExpenseState(this);
            
            else
            {
                var start = stateFactory.WayOfEnteringExpenseState(this);
                return start.MoveToNextState(message, stateFactory, cancellationToken);
            }
        }
    }
}