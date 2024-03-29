using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class EnterSubcategoryState : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly SubCategory[] _subCategories;
        private readonly ExpenseBuilder _expenseBuilder;
        private readonly ILogger _logger;
    
        public IExpenseInfoState PreviousState { get; private set; }
    
        internal EnterSubcategoryState(StateFactory factory, IExpenseInfoState previousState, ExpenseBuilder builder, SubCategory[] subCategories, ILogger logger)
        {
            _factory = factory;
            _subCategories = subCategories;
            _expenseBuilder = builder;
            _logger = logger;
            PreviousState = previousState;
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

        public Task Handle(IMessage message, CancellationToken cancellationToken)
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

        public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
        {
            if (_expenseBuilder.SubCategory != null)
            {
                return _factory.CreateEnterDescriptionState(_expenseBuilder, this);
            }

            return this;
        }
    }
}