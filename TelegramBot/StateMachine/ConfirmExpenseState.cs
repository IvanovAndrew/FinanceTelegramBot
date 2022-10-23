using System;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using GoogleSheet;
using Microsoft.Extensions.Logging;
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

    public bool AnswerIsRequired => true;

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        if (string.Equals(text, "Save", StringComparison.InvariantCultureIgnoreCase))
        {
            return new SaveExpenseState(_expense, _spreadsheetWriter, _logger);
        }
            
        if (string.Equals(text, "Cancel", StringComparison.InvariantCultureIgnoreCase))
        {
            return new CancelExpenseState();
        }

        return null;
    }
}