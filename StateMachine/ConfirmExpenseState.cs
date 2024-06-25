using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class ConfirmExpenseState : IExpenseInfoState
    {
        private readonly IExpense _expense;
        private readonly IEnumerable<Category> _categories;
        private readonly ILogger _logger;
        public bool UserAnswerIsRequired => true;
    
        public ConfirmExpenseState(IExpense expense, IEnumerable<Category> categories, ILogger logger)
        {
            _expense = expense;
            _logger = logger;
            _categories = categories;
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

        public Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            var category = _categories.FirstOrDefault(c => c.Name == _expense.Category);
            SubCategory? subCategory = null;

            if (category != null && _expense.SubCategory != null)
            {
                subCategory = category.SubCategories.FirstOrDefault(c => c.Name == _expense.SubCategory);
            }
                
            var expenseBuilder = new ExpenseBuilder()
            {
                Category = category,
                SubCategory = subCategory,
                Date = _expense.Date,
                Description = _expense.Description, 
            };
            
            return stateFactory.CreateEnterThePriceState(expenseBuilder);
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            if (string.Equals(message.Text, "Save", StringComparison.InvariantCultureIgnoreCase))
            {
                return stateFactory.CreateSaveState(_expense);
            }
            
            if (string.Equals(message.Text, "Cancel", StringComparison.InvariantCultureIgnoreCase))
            {
                return stateFactory.CreateCancelState();
            }

            throw new BotStateException(new []{"Save", "Cancel"}, message.Text);
        }
    }
}