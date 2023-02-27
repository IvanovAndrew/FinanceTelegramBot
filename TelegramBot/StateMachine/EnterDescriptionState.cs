using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

class EnterDescriptionState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly ExpenseBuilder _expenseBuilder;
    private readonly ILogger _logger;
    
    public IExpenseInfoState PreviousState { get; private set; }
    
    internal EnterDescriptionState(StateFactory factory, IExpenseInfoState previousState, ExpenseBuilder expenseBuilder, ILogger logger)
    {
        _factory = factory;
        _expenseBuilder = expenseBuilder;
        _logger = logger;

        PreviousState = previousState;
    }

    public bool UserAnswerIsRequired => true;

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        return await botClient.SendTextMessageAsync(chatId, "Write description", cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        _expenseBuilder.Description = text; 
        return _factory.CreateEnterThePriceState(_expenseBuilder, this);
    }
}