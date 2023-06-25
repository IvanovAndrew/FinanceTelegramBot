using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;
using TelegramBot;

namespace StateMachine
{
    class EnterTheCategoryState : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly ExpenseBuilder _expenseBuilder;
        private readonly IEnumerable<Category> _categories;
        private readonly ILogger _logger;
    
        public IExpenseInfoState PreviousState { get; private set; }

        internal EnterTheCategoryState(StateFactory stateFactory, IExpenseInfoState previousState, ExpenseBuilder expenseBuilder, IEnumerable<Category> categories, ILogger logger)
        {
            _factory = stateFactory;
            _expenseBuilder = expenseBuilder;
            _categories = categories;
            _logger = logger;

            PreviousState = previousState;
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

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            var categoryDomain = _categories.FirstOrDefault(c => c.Name == text);
            if (categoryDomain != null)
            {
                _expenseBuilder.Category = categoryDomain;
                if (categoryDomain.SubCategories.Any())
                {
                    return _factory.CreateEnterTheSubcategoryState(_expenseBuilder, this, categoryDomain.SubCategories);
                }
                else
                {
                    return _factory.CreateEnterDescriptionState(_expenseBuilder, this);
                }
            }

            return this;
        }
    }
}