using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.StateMachine
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
    
        public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var info = "Choose kind of statistic";

            var buttons = new[]
            {
                InlineKeyboardButton.WithCallbackData(text: $"For a day", callbackData: "statisticByDay"),
                InlineKeyboardButton.WithCallbackData(text: $"For a month", callbackData: "statisticByMonth"), 
                InlineKeyboardButton.WithCallbackData(text: $"For a category", callbackData: "statisticByCategory") 
            };
        
            InlineKeyboardMarkup inlineKeyboard = new(buttons);
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: info,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            if (text == "statisticByDay") return _factory.CreateEnterTheExpenseDayState(this);
            if (text == "statisticByMonth") return _factory.CreateEnterTheMonthState(this);
            if (text == "statisticByCategory") return _factory.CreateCategoryForStatisticState(this);

            throw new ArgumentOutOfRangeException(text);
        }
    }
}