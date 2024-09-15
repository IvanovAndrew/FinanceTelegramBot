using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class CreateStatisticTypeState : IExpenseInfoState
    {
        private readonly ILogger _logger;
    
        public bool UserAnswerIsRequired => true;
    
        public CreateStatisticTypeState(ILogger logger)
        {
            _logger = logger;
        }
    
        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            var info = "Choose kind of statistic";

            var keyboard = 
                TelegramKeyboard.FromButtons(new []
                {
                    new TelegramButton { Text = "Day expenses (by categories)", CallbackData = "statisticByDay"},
                    new TelegramButton { Text = "Month expenses (by categories)", CallbackData = "statisticByMonth"}, 
                    new TelegramButton { Text = "Category expenses (by months)", CallbackData = "statisticByCategory"}, 
                    new TelegramButton { Text = "Subcategory expenses (overall)", CallbackData = "statisticBySubcategory"}, 
                    new TelegramButton { Text = "Subcategory expenses (by months)", CallbackData = "statisticBySubcategoryByMonth"}, 
                }, chunkSize:2);
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: info,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            return stateFactory.CreateGreetingState();
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            if (message.Text == "statisticByDay") return stateFactory.CreateCollectDayExpenseState();
            if (message.Text == "statisticByMonth") return stateFactory.CreateCollectMonthStatisticState();
            if (message.Text == "statisticByCategory") return stateFactory.CreateCollectCategoryExpensesByMonthsState();
            if (message.Text == "statisticBySubcategory") return stateFactory.CreateCollectSubcategoriesForAPeriodState();
            if (message.Text == "statisticBySubcategoryByMonth") return stateFactory.CreateCollectSubcategoryExpensesByMonthsState();

            throw new BotStateException(new []{"balance", "statisticByDay", "statisticByMonth", "statisticByCategory", "statisticBySubcategory", "statisticBySubcategoryByMonth"}, message.Text);
        }
    }
}