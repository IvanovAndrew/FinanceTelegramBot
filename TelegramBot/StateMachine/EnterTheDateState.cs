using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.StateMachine
{
    class EnterTheDateState : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly IDateParser _dateParser;
        private readonly ILogger _logger;
        private readonly bool _askCustomDate;
    
        public IExpenseInfoState PreviousState { get; private set; }
        public bool UserAnswerIsRequired => true;
    
        public EnterTheDateState(StateFactory factory, IExpenseInfoState previousState, IDateParser dateParser, ILogger logger, bool askCustomDate = false)
        {
            _factory = factory;
            _dateParser = dateParser;
            _askCustomDate = askCustomDate;
            _logger = logger;
            PreviousState = previousState;
        }
    

        public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var info = "Enter the date";

            if (_askCustomDate)
            {
                return await botClient.SendTextMessageAsync(chatId, $"{info}", cancellationToken: cancellationToken);
            }
        
            InlineKeyboardMarkup inlineKeyboard = new(
                // keyboard
                new[]
                {
                    // first row
                    InlineKeyboardButton.WithCallbackData(text:"Today", callbackData:"today"),
                    InlineKeyboardButton.WithCallbackData(text:"Yesterday", callbackData:"yesterday"),
                    InlineKeyboardButton.WithCallbackData(text:"Other", callbackData:"Other"),
                });
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: info,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            var expenseBuilder = new ExpenseBuilder();

            if (text.ToLowerInvariant() == "other")
            {
                return _factory.CreateEnterTheDateState(this, true);
            }
        
            if (!_dateParser.TryParse(text, out var date))
            {
                _logger.LogDebug($"{text} isn't a date");
                return _factory.CreateErrorWithRetryState($"{text} isn't a date.", this);
            }

            expenseBuilder.Date = date;
            return _factory.CreateEnterTheCategoryState(expenseBuilder, this);
        }
    }
}