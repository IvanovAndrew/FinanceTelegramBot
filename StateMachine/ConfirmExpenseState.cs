using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    abstract class ConfirmState<T> : IExpenseInfoState
    {
        protected readonly T _item;
        
        private readonly ILogger _logger;
        public bool UserAnswerIsRequired => true;
    
        public ConfirmState(T item, ILogger logger)
        {
            _item = item;
            _logger = logger;
        }

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            string infoMessage = string.Join($"{Environment.NewLine}", 
                ItemToString(),
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

        public abstract IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory);

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            if (string.Equals(message.Text, "Save", StringComparison.InvariantCultureIgnoreCase))
            {
                return ToSaveState(stateFactory);
            }
            
            if (string.Equals(message.Text, "Cancel", StringComparison.InvariantCultureIgnoreCase))
            {
                return stateFactory.CreateCancelState();
            }

            throw new BotStateException(new []{"Save", "Cancel"}, message.Text);
        }

        protected abstract IExpenseInfoState ToSaveState(IStateFactory stateFactory);
        protected abstract string ItemToString();
    }

    class ConfirmExpenseState : ConfirmState<IExpense>
    {
        private readonly IEnumerable<Category> _categories;
        
        public ConfirmExpenseState(IExpense expense, IEnumerable<Category> categories, ILogger logger) : base(expense, logger)
        {
            _categories = categories;
        }

        public override IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            var category = _categories.FirstOrDefault(c => c.Name == _item.Category);
            SubCategory? subCategory = null;

            if (category != null && _item.SubCategory != null)
            {
                subCategory = category.Subcategories.FirstOrDefault(c => c.Name == _item.SubCategory);
            }
                
            var expenseBuilder = new ExpenseBuilder()
            {
                Category = category,
                SubCategory = subCategory,
                Date = _item.Date,
                Description = _item.Description, 
            };
            
            return stateFactory.CreateEnterOutcomeManuallyState();
            
        }

        protected override IExpenseInfoState ToSaveState(IStateFactory stateFactory)
        {
            return stateFactory.CreateSaveState(_item);
        }

        protected override string ItemToString()
        {
            return string.Join($"{Environment.NewLine}",
                $"Date: {_item.Date:dd.MM.yyyy}",
                $"Category: {_item.Category}",
                $"SubCategory: {_item.SubCategory ?? string.Empty}",
                $"Description: {_item.Description ?? string.Empty}",
                $"Amount: {_item.Amount}");
        }
    }

    class ConfirmIncomeState : ConfirmState<IIncome>
    {
        public ConfirmIncomeState(IIncome income, ILogger logger) : base(income, logger)
        {
        }

        public override IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            throw new NotImplementedException();
        }

        protected override IExpenseInfoState ToSaveState(IStateFactory stateFactory)
        {
            return stateFactory.CreateSaveState(_item);
        }

        protected override string ItemToString()
        {
            return string.Join($"{Environment.NewLine}",
                $"Date: {_item.Date:dd.MM.yyyy}",
                $"Category: {_item.Category}",
                $"Description: {_item.Description}",
                $"Amount: {_item.Amount}");
        }
    }
}