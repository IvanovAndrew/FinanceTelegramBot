using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using GoogleSheet;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.StateMachine;

class EnterSubcategoryState : IExpenseInfoState
{
    private readonly ExpenseBuilder _expenseBuilder;
    private readonly GoogleSheetWriter _spreadsheetWriter;
    private readonly IMoneyParser _moneyParser;
    private readonly ILogger _logger;
    
    internal EnterSubcategoryState(ExpenseBuilder builder, IMoneyParser moneyParser, GoogleSheetWriter spreadsheetWriter, ILogger logger)
    {
        _expenseBuilder = builder;
        _moneyParser = moneyParser;
        _spreadsheetWriter = spreadsheetWriter;
        _logger = logger;
    }

    public bool UserAnswerIsRequired => true;

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var category = _expenseBuilder.Category!;
        var firstRow = category.SubCategories.Take(4);
        var secondRow = Enumerable.Empty<SubCategory>();
        if (category.SubCategories.Length > 4)
        {
            secondRow = category.SubCategories.Skip(4).Take(4);
        }    
        
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
            text: "Choose the subcategory",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        var subCategory = _expenseBuilder.Category.SubCategories.FirstOrDefault(c => c.Name == text);
        if (subCategory != null)
        {
            _expenseBuilder.SubCategory = subCategory;
            return new EnterDescriptionState(_expenseBuilder, _moneyParser, _spreadsheetWriter, _logger);
        }

        return this;
    }
}