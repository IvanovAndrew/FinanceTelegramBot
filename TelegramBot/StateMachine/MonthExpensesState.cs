using System.Text;
using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;
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

        var dictionaryCategoryToSum = new Dictionary<string, Money>();
        Money total = new Money { Currency = Currency.Amd, Amount = 0m };
        
        foreach (var row in rows)
        {
            if (row.Amount.Currency != Currency.Amd) continue;
            
            if (dictionaryCategoryToSum.TryGetValue(row.Category, out var sum))
            {
                dictionaryCategoryToSum[row.Category] = sum + row.Amount;
            }
            else
            {
                dictionaryCategoryToSum[row.Category] = row.Amount;
            }

            total += row.Amount;
        }

        var stringBuilder = new StringBuilder();
        foreach ((string category, Money sum) in dictionaryCategoryToSum.OrderByDescending(kvp => kvp.Value.Amount))
        {
            stringBuilder.AppendLine($"{category} {sum}");
        }

        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"Total {total}");

        return await botClient.SendTextMessageAsync(chatId: chatId, stringBuilder.ToString(),
            cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        return _factory.CreateGreetingState();
    }
}