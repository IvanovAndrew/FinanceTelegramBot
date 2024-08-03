using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class EnterCategoryForStatisticState : IExpenseInfoState
    {
        private readonly IEnumerable<Category> _categories;
        private ILogger _logger;

        public EnterCategoryForStatisticState(IEnumerable<Category> categories, ILogger logger)
        {
            _logger = logger;
            _categories = categories;
        }

        public bool UserAnswerIsRequired { get; } = true;

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

        public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            return stateFactory.CreateChooseStatisticState();
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            var categoryDomain = _categories.FirstOrDefault(c => c.Name == message.Text);
            if (categoryDomain != null)
            {
                return stateFactory.CreateEnterTypeOfCategoryStatistic(categoryDomain, this);
            }

            return this;
        }
    }
}