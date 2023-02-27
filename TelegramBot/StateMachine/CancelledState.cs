using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

class CancelledState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly ILogger _logger;
    
    public IExpenseInfoState PreviousState { get; private set; }

    public CancelledState(StateFactory factory, ILogger logger)
    {
        _factory = factory;
        _logger = logger;
    }
    
    public bool UserAnswerIsRequired => false;
    public Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        return botClient.SendTextMessageAsync(chatId, "Canceled");
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}