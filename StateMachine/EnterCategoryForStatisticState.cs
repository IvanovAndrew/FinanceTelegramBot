using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class EnterCategoryForStatisticState : IExpenseInfoState
    {
        private StateFactory _factory;
        private readonly IEnumerable<Category> _categories;
        private ILogger _logger;

        public EnterCategoryForStatisticState(StateFactory factory, IExpenseInfoState previousState,
            IEnumerable<Category> categories, ILogger logger)
        {
            _factory = factory;
            PreviousState = previousState;
            _logger = logger;
            _categories = categories;
        }

        public bool UserAnswerIsRequired { get; } = true;
        public IExpenseInfoState PreviousState { get; }

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            string infoMessage = "Enter the category";

            var keyboard = TelegramKeyboard.FromButtons(
                _categories.Select(c => new TelegramButton() { Text = c.Name, CallbackData = c.Name })
            );

            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: infoMessage,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(IMessage message, CancellationToken cancellationToken)
        {
            var categoryDomain = _categories.FirstOrDefault(c => c.Name == message.Text);
            if (categoryDomain != null)
            {
                return _factory.GetEnterTypeOfCategoryStatistic(this, categoryDomain);
            }

            return this;
        }
    }
}