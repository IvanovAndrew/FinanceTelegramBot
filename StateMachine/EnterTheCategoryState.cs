using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class EnterTheCategoryState : IExpenseInfoState
    {
        private readonly ExpenseBuilder _expenseBuilder;
        private readonly IEnumerable<Category> _categories;
        private readonly ILogger _logger;
    
        internal EnterTheCategoryState(ExpenseBuilder expenseBuilder, IEnumerable<Category> categories, ILogger logger)
        {
            _expenseBuilder = expenseBuilder;
            _categories = categories;
            _logger = logger;
        }

        public bool UserAnswerIsRequired => true;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            string infoMessage = "Enter the category";

            // keyboard
            var keyboard = TelegramKeyboard.FromButtons(_categories.Select(c => new TelegramButton()
                { Text = c.Name, CallbackData = c.Name })); 
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: infoMessage,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public async Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            var action = () =>
            {
                var categoryDomain = _categories.FirstOrDefault(c =>
                    string.Equals(c.Name, message.Text, StringComparison.InvariantCultureIgnoreCase));
                if (categoryDomain != null)
                {
                    _expenseBuilder.Category = categoryDomain;
                }
            };

            await Task.Run(action);
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            return stateFactory.CreateEnterTheDateState(this, false);
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            var categoryDomain = _expenseBuilder.Category;
            if (categoryDomain != null)
            {
                if (categoryDomain.Subcategories.Any())
                {
                    return stateFactory.CreateEnterTheSubcategoryState(_expenseBuilder,  categoryDomain.Subcategories);
                }
                else
                {
                    return stateFactory.CreateEnterDescriptionState(_expenseBuilder);
                }
            }

            return this;
        }
    }
}