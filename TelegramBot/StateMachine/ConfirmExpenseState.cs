using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
        string s = string.Join(", ", 
            $"{_expense.Date:dd.MM.yyyy}", 
            $"{_expense.Category}", 
            $"{_expense.SubCategory ?? string.Empty}", 
            $"{_expense.Description ?? string.Empty}",
            $"{_expense.Amount}"
        );
        string infoMessage = $"Check your data: {s}";
        
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
            text: $"{infoMessage}. Can I save it?",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        if (text == "Save")
        {
            return new SaveExpenseState(_expense, _spreadsheetWriter, _logger);
        }
            
        if (text == "Cancel")
        {
            return new CancelledState(_logger);
        }

        throw new ArgumentOutOfRangeException(nameof(text), $@"Expected values are ""Save"" or ""Cancel"". {text} was received.");
    }
}