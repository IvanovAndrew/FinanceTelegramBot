using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.StateMachine;

namespace TelegramBot.StateMachine;

class ConfirmExpenseState : IExpenseInfoState
{
    private readonly IExpense _expense;
    private readonly GoogleSheetWriter _spreadsheetWriter;
    private readonly ILogger _logger;
    public ConfirmExpenseState(IExpense expense, GoogleSheetWriter spreadsheetWriter, ILogger logger)
    {
        _expense = expense;
        _spreadsheetWriter = spreadsheetWriter;
        _logger = logger;
    }

    public bool UserAnswerIsRequired => true;

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        string infoMessage = string.Join($"{Environment.NewLine}", 
            $"Date: {_expense.Date:dd.MM.yyyy}", 
            $"Category: {_expense.Category}", 
            $"SubCategory: {_expense.SubCategory ?? string.Empty}", 
            $"Description: {_expense.Description ?? string.Empty}",
            $"Amount: {_expense.Amount}",
            "",
            "Would you like to save it?"
        );

        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
            // keyboard
            new[]
            {
                // first row
                new[]
                {
                    // first button in row
                    InlineKeyboardButton.WithCallbackData(text: "Save", callbackData: "Save"),
                    InlineKeyboardButton.WithCallbackData(text: "Cancel", callbackData: "Cancel"),
                }
            });

        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"{infoMessage}",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    public bool AnswerIsRequired => true;

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        if (string.Equals(text, "Save", StringComparison.InvariantCultureIgnoreCase))
        {
            return new SaveExpenseState(_expense, _spreadsheetWriter, _logger);
        }
            
        if (string.Equals(text, "Cancel", StringComparison.InvariantCultureIgnoreCase))
        {
            return new CancelledState(_logger);
        }

        throw new ArgumentOutOfRangeException(nameof(text), $@"Expected values are ""Save"" or ""Cancel"". {text} was received.");
    }
}