using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using GoogleSheet;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.StateMachine;

class EnterTheDateState : IExpenseInfoState
{
    private readonly DateOnly _today;
    private readonly IMoneyParser _moneyParser;
    private readonly IEnumerable<Category> _categories;
    private readonly GoogleSheetWriter _spreadsheetWriter;
    private readonly ILogger _logger;
    private readonly bool _askCustomDate;
    private const string DateFormat = "MM.dd.yyyy";
    
    public EnterTheDateState(DateOnly today, IEnumerable<Category> categories, IMoneyParser moneyParser, GoogleSheetWriter spreadsheetWriter, ILogger logger, bool askCustomDate = false)
    {
        _today = today;
        _categories = categories;
        _spreadsheetWriter = spreadsheetWriter;
        _moneyParser = moneyParser;
        _askCustomDate = askCustomDate;
        _logger = logger;
    }

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var info = "Enter the date";

        if (_askCustomDate)
        {
            return await botClient.SendTextMessageAsync(chatId, $"{info} ({DateFormat})", cancellationToken: cancellationToken);
        }
        
        InlineKeyboardMarkup inlineKeyboard = new(
            // keyboard
            new[]
            {
                // first row
                InlineKeyboardButton.WithCallbackData(text:"Today", callbackData:_today.ToString(DateFormat)),
                InlineKeyboardButton.WithCallbackData(text:"Yesterday", callbackData:_today.AddDays(-1).ToString(DateFormat)),
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
            return new EnterTheDateState(_today, _categories, _moneyParser, _spreadsheetWriter, _logger, true);
        }
        
        if (!DateOnly.TryParse(text, out var date))
        {
            _logger.LogDebug($"{text} isn't a date");
            return new ErrorWithRetry($"{text} isn't a date.", this);
        }

        expenseBuilder.Date = date;

        return new EnterTheCategoryState(expenseBuilder, _categories, _moneyParser, _spreadsheetWriter, _logger);
    }
}