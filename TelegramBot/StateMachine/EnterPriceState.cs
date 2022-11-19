using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

class EnterPriceState : IExpenseInfoState
{
    private readonly ExpenseBuilder _expenseBuilder;
    private readonly GoogleSheetWriter _spreadsheetWriter;
    private readonly IMoneyParser _moneyParser;
    private readonly ILogger _logger;

    internal EnterPriceState(ExpenseBuilder expenseBuilder, IMoneyParser moneyParser, GoogleSheetWriter spreadsheetWriter, ILogger logger)
    {
        _expenseBuilder = expenseBuilder;
        _moneyParser = moneyParser;
        _spreadsheetWriter = spreadsheetWriter;
        _logger = logger;
    }

    public bool UserAnswerIsRequired => true;

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        return await botClient.SendTextMessageAsync(chatId, "Enter the price", cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        if (!_moneyParser.TryParse(text, out var money))
        {
            string warning = $"{text} wasn't recognized as money.";
            _logger.LogDebug(warning);
            
            return new ErrorWithRetry(warning, this);
        }

        _expenseBuilder.Sum = money;

        return new ConfirmExpenseState(_expenseBuilder.Build(), _spreadsheetWriter, _logger);
    }
}