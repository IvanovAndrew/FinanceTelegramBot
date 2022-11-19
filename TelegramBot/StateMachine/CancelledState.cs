using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

class CancelledState : IExpenseInfoState
{
    private readonly ILogger _logger;

    public CancelledState(ILogger logger)
    {
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