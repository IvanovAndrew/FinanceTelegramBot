using System.Threading;
using System.Threading.Tasks;
using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

class EnterPriceState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly ExpenseBuilder _expenseBuilder;
    private readonly IMoneyParser _moneyParser;
    private readonly ILogger _logger;
    
    public IExpenseInfoState PreviousState { get; private set; }

    internal EnterPriceState(StateFactory factory, IExpenseInfoState previousState, ExpenseBuilder expenseBuilder, IMoneyParser moneyParser, ILogger logger)
    {
        _factory = factory;
        _expenseBuilder = expenseBuilder;
        _moneyParser = moneyParser;
        _logger = logger;
        PreviousState = previousState;
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
            
            return _factory.CreateErrorWithRetryState(warning, this);
        }

        _expenseBuilder.Sum = money;

        return _factory.CreateConfirmState(_expenseBuilder.Build(), this);
    }
}