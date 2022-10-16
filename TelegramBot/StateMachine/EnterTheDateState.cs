using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

class EnterTheDateState : IExpenseInfoState
{
    private readonly IMoneyParser _moneyParser;
    private readonly IEnumerable<Category> _categories;
    private readonly GoogleSheetWriter _spreadsheetWriter;
    private readonly ILogger _logger;
    
    public EnterTheDateState(IEnumerable<Category> categories, IMoneyParser moneyParser, GoogleSheetWriter spreadsheetWriter, ILogger logger)
    {
        _categories = categories;
        _spreadsheetWriter = spreadsheetWriter;
        _moneyParser = moneyParser;
        _logger = logger;
    }

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        return await botClient.SendTextMessageAsync(chatId: chatId, "Enter the date:", cancellationToken:cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        var expenseBuilder = new ExpenseBuilder();
        DateOnly date;
        
        if (string.Equals("today", text, StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals("сегодня", text, StringComparison.InvariantCultureIgnoreCase))
        {
            date = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        }
        else if (string.Equals("yesterday", text, StringComparison.InvariantCultureIgnoreCase) ||
                 string.Equals("вчера", text, StringComparison.InvariantCultureIgnoreCase))
        {
            var dateTime = DateTime.Now.AddDays(-1);
            date = new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
        }
        else if (!DateOnly.TryParse(text, out date))
        {
            _logger.LogDebug($"{text} isn't a date");
            return new ErrorWithRetry($"{text} isn't a date.", this);
        }

        expenseBuilder.Date = date;

        return new EnterTheCategoryState(expenseBuilder, _categories, _moneyParser, _spreadsheetWriter, _logger);
    }
}