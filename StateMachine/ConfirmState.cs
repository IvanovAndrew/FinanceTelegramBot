using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class ConfirmState : IExpenseInfoState
    {
        private readonly IMoneyTransfer _item;
        
        private readonly ILogger _logger;
        public bool UserAnswerIsRequired => true;
    
        public ConfirmState(IMoneyTransfer item, ILogger logger)
        {
            _item = item;
            _logger = logger;
        }

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            string infoMessage = string.Join($"{Environment.NewLine}", 
                _item.ToString(),
                "",
                "Would you like to save it?"
            );

            var keyboard = TelegramKeyboard.FromButtons(
                new[]
                {
                    new TelegramButton() { Text = "Save", CallbackData = "Save" },
                    new TelegramButton() { Text = "Cancel", CallbackData = "Cancel" },
                });

            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"{infoMessage}",
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            return _item.IsIncome? stateFactory.CreateEnterIncomeState() : stateFactory.CreateEnterOutcomeManuallyState();
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            if (string.Equals(message.Text, "Save", StringComparison.InvariantCultureIgnoreCase))
            {
                return stateFactory.CreateSaveState(_item);;
            }
            
            if (string.Equals(message.Text, "Cancel", StringComparison.InvariantCultureIgnoreCase))
            {
                return stateFactory.CreateCancelState();
            }

            throw new BotStateException(new []{"Save", "Cancel"}, message.Text);
        }
    }
}