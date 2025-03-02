using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class GreetingState(ILogger logger) : IExpenseInfoState
    {
        public bool UserAnswerIsRequired => true;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                new EditableMessageToSend()
                {
                    ChatId = chatId, 
                    Text = "What would you like to do?", 
                    Keyboard = TelegramKeyboard.FromButtons(new[]
                    {
                        new TelegramButton() { Text = "Outcome", CallbackData = "startExpense" },
                        new TelegramButton() { Text = "Income", CallbackData = "startIncome" },
                        new TelegramButton() { Text = "Statistics", CallbackData = "showExpenses" },
                    }),
                }, cancellationToken: cancellationToken);
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