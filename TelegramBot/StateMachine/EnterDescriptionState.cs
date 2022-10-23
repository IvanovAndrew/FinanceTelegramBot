using System.Threading;
using System.Threading.Tasks;
using Domain;
using GoogleSheet;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

class EnterDescriptionState : IExpenseInfoState
{
    private readonly ExpenseBuilder _expenseBuilder;
    private readonly GoogleSheetWriter _spreadsheetWriter;
    private readonly IMoneyParser _moneyParser;
    private readonly ILogger _logger;
    
    internal EnterDescriptionState(ExpenseBuilder expenseBuilder, IMoneyParser moneyParser, GoogleSheetWriter spreadsheetWriter, ILogger logger)
    {
        _expenseBuilder = expenseBuilder;
        _moneyParser = moneyParser;
        _spreadsheetWriter = spreadsheetWriter;
        _logger = logger;
    }

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        return await botClient.SendTextMessageAsync(chatId, "Write description", cancellationToken: cancellationToken);
    }

    public bool AnswerIsRequired => true;

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        _expenseBuilder.Description = text; 
        return new EnterPriceState(_expenseBuilder, _moneyParser, _spreadsheetWriter, _logger);
    }
}