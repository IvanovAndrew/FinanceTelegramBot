using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class CreateStatisticTypeState : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly ILogger _logger;
    
        public bool UserAnswerIsRequired => true;
        public IExpenseInfoState PreviousState { get; }
    
        public CreateStatisticTypeState(StateFactory factory, IExpenseInfoState previousState, ILogger logger)
        {
            _factory = factory;
            _logger = logger;
            PreviousState = previousState;
        }
    
        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            var info = "Choose kind of statistic";

            var keyboard = 
                TelegramKeyboard.FromButtons(new []
                {
                    new TelegramButton { Text = "For a day", CallbackData = "statisticByDay"},
                    new TelegramButton { Text = "For a month", CallbackData = "statisticByMonth"}, 
                    new TelegramButton { Text = "For a category", CallbackData = "statisticByCategory"} 
                });
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: info,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public async Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Run(() => { });
        }

        public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
        {
            if (message.Text == "statisticByDay") return _factory.CreateCollectDayExpenseState(this);
            if (message.Text == "statisticByMonth") return _factory.CreateCollectMonthStatisticState(this);
            if (message.Text == "statisticByCategory") return _factory.CreateCategoryForStatisticState(this);

            throw new BotStateException(new []{"statisticByDay", "statisticByMonth", "statisticByCategory"}, message.Text);
        }
    }
}