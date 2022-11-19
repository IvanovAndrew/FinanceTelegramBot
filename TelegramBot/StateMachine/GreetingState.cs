using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.StateMachine;

class GreetingState : IExpenseInfoState
{
    private readonly DateOnly _today;
    private readonly IEnumerable<Category> _categories;
    private readonly IMoneyParser _moneyParser;
    private readonly GoogleSheetWriter _spreadsheetWriter;
    private readonly ILogger _logger;
    
    public GreetingState(DateOnly today, IEnumerable<Category> categories, IMoneyParser moneyParser,
        GoogleSheetWriter spreadsheetWriter, ILogger logger)
    {
        _today = today;
        _categories = categories;
        _moneyParser = moneyParser;
        _spreadsheetWriter = spreadsheetWriter;
        _logger = logger;
    }

    public bool UserAnswerIsRequired => true;

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
            new[] { InlineKeyboardButton.WithCallbackData(text: "Outcome", callbackData: "startExpense") }
        );

        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "What would you like to do?",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        return new EnterTheDateState(_today, _categories, _moneyParser, _spreadsheetWriter, _logger);
    }
}