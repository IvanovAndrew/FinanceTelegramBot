using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

internal class ErrorWithRetry : IExpenseInfoState
{
    private readonly string _errorMessage;
    private readonly IExpenseInfoState _state;
    
    internal ErrorWithRetry(string message, IExpenseInfoState state)
    {
        _errorMessage = message;
        _state = state;
    }

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId, _errorMessage, cancellationToken: cancellationToken);

        return await _state.Request(botClient, chatId, cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        return _state.Handle(text, cancellationToken);
    }
}