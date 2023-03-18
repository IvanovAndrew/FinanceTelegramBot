﻿using System.Text;
using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.StateMachine;

namespace TelegramBot;

internal class MonthExpensesState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly DateOnly _selectedMonth;
    private readonly GoogleSheetWrapper _spreadsheetWrapper;
    private readonly ILogger _logger;
    
    public IExpenseInfoState PreviousState { get; private set; }
    public MonthExpensesState(StateFactory stateFactory, IExpenseInfoState previousState, DateOnly selectedMonth, GoogleSheetWrapper spreadsheetWrapper, ILogger logger)
    {
        _factory = stateFactory;
        _selectedMonth = selectedMonth;
        _spreadsheetWrapper = spreadsheetWrapper;
        _logger = logger;
        PreviousState = previousState;
    }

    public bool UserAnswerIsRequired => false;
    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Collecting expenses... It can take some time.");
        await botClient.SendTextMessageAsync(chatId: chatId, "Collecting expenses... It can take some time.");
        
        var rows = await _spreadsheetWrapper.GetRows(_selectedMonth, cancellationToken);
        
        _logger.LogInformation($"Found {rows.Count} row(s) for month {_selectedMonth}");

        Message lastMessage = default;
        foreach (var currency in new []{Currency.Amd, Currency.Rur})
        {
            var (categories, total) = SumByCategories(rows, currency);
            string[,] telegramTable = new string[categories.Count + 2, 2];
            int i = 0;
            foreach ((string category, Money sum) in categories.OrderByDescending(kvp => kvp.Value.Amount))
            {
                telegramTable[i, 0] = category;
                telegramTable[i, 1] = sum.ToString();
                i++;
            }
        
            telegramTable[categories.Count, 0] = new string('=', categories.Keys.MaxBy(s => s.Length)?.Length?? 5);
            telegramTable[categories.Count, 1] = "";

            telegramTable[categories.Count + 1, 0] = "Всего";
            telegramTable[categories.Count + 1, 1] = total.ToString();
            
            var table = MarkdownFormatter.FormatTable(new[] { "Category", "Sum" }, telegramTable);

            lastMessage = await botClient.SendTextMessageAsync(chatId: chatId, $"```{TelegramEscaper.EscapeString(table)}```",
                cancellationToken: cancellationToken, parseMode: ParseMode.MarkdownV2);
        }

        return lastMessage;
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        return _factory.CreateGreetingState();
    }

    private (Dictionary<string, Money>, Money total) SumByCategories(IEnumerable<IExpense> expenses, Currency currency)
    {
        var categoriesSum = new Dictionary<string, Money>();
        Money total = new Money { Currency = currency, Amount = 0m };
        
        foreach (var row in expenses)
        {
            if (row.Amount.Currency != currency) continue;
            
            if (categoriesSum.TryGetValue(row.Category, out var sum))
            {
                categoriesSum[row.Category] = sum + row.Amount;
            }
            else
            {
                categoriesSum[row.Category] = row.Amount;
            }

            total += row.Amount;
        }

        return (categoriesSum, total);
    }
}