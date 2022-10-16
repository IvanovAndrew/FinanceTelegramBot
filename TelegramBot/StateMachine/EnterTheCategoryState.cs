using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.StateMachine;

class EnterTheCategoryState : IExpenseInfoState
{
    private readonly ExpenseBuilder _expenseBuilder;
    private readonly IEnumerable<Category> _categories;
    private readonly GoogleSheetWriter _spreadsheetWriter;
    private readonly IMoneyParser _moneyParser;
    private readonly ILogger _logger;

    internal EnterTheCategoryState(ExpenseBuilder expenseBuilder, IEnumerable<Category> categories, IMoneyParser moneyParser, GoogleSheetWriter spreadsheetWriter, ILogger logger)
    {
        _expenseBuilder = expenseBuilder;
        _categories = categories;
        _spreadsheetWriter = spreadsheetWriter;
        _moneyParser = moneyParser;
        _logger = logger;
    }

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        string infoMessage = "Enter the category";

        var firstRow = _categories.Take(4);
        var secondRow = _categories.Skip(4).Take(4);
        
        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
            // keyboard
            new[]
            {
                // first row
                firstRow.Select(c => InlineKeyboardButton.WithCallbackData(text:c.Name, callbackData:c.Name)).ToArray(),
                secondRow.Select(c => InlineKeyboardButton.WithCallbackData(text:c.Name, callbackData:c.Name)).ToArray(),
            });
        
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: infoMessage,
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        var categoryDomain = _categories.FirstOrDefault(c => c.Name == text);
        if (categoryDomain != null)
        {
            _expenseBuilder.Category = categoryDomain;
            if (categoryDomain.SubCategories.Any())
            {
                return new EnterSubcategoryState(_expenseBuilder, _moneyParser, _spreadsheetWriter, _logger);
            }
            else
            {
                return new EnterDescriptionState(_expenseBuilder, _moneyParser, _spreadsheetWriter, _logger);
            }
        }

        return this;
    }
}