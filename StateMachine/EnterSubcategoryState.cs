using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class EnterSubcategoryState : IExpenseInfoState
    {
        private readonly SubCategory[] _subCategories;
        private readonly ExpenseBuilder _expenseBuilder;
        private readonly ILogger _logger;
    
        internal EnterSubcategoryState(ExpenseBuilder builder, SubCategory[] subCategories, ILogger logger)
        {
            _subCategories = subCategories;
            _expenseBuilder = builder;
            _logger = logger;
        }

        public bool UserAnswerIsRequired => true;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            var keyboard = TelegramKeyboard.FromButtons(_subCategories.Select(c => new TelegramButton()
                { Text = c.Name, CallbackData = c.Name }));

            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Choose the subcategory",
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            var handle = () =>
            {
                var subCategory = _subCategories.FirstOrDefault(c => c.Name == message.Text);
                if (subCategory != null)
                {
                    _expenseBuilder.SubCategory = subCategory;
                }
            };
                
            return Task.Run(handle);
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) => stateFactory.CreateEnterTheCategoryState(_expenseBuilder);

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            if (_expenseBuilder.SubCategory != null)
            {
                return stateFactory.CreateEnterDescriptionState(_expenseBuilder);
            }

            return this;
        }
    }
}