using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class ConfirmExpenseState : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly IExpense _expense;
        private readonly ILogger _logger;
        public IExpenseInfoState PreviousState { get; }
        public bool UserAnswerIsRequired => true;
    
        public ConfirmExpenseState(StateFactory stateFactory, IExpenseInfoState previousState, IExpense expense, ILogger logger)
        {
            _factory = stateFactory;
            _expense = expense;
            _logger = logger;
            PreviousState = previousState;
        }

    

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            string infoMessage = string.Join($"{Environment.NewLine}", 
                $"Date: {_expense.Date:dd.MM.yyyy}", 
                $"Category: {_expense.Category}", 
                $"SubCategory: {_expense.SubCategory ?? string.Empty}", 
                $"Description: {_expense.Description ?? string.Empty}",
                $"Amount: {_expense.Amount}",
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

        public async Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Run(() => { });
        }

        public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
        {
            if (string.Equals(message.Text, "Save", StringComparison.InvariantCultureIgnoreCase))
            {
                return _factory.CreateSaveState(this, _expense);
            }
            
            if (string.Equals(message.Text, "Cancel", StringComparison.InvariantCultureIgnoreCase))
            {
                return _factory.CreateCancelState();
            }

            throw new BotStateException(new []{"Save", "Cancel"}, message.Text);
        }
    }
}