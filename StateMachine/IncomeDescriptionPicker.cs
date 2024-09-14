using Infrastructure;

namespace StateMachine;

class IncomeDescription : IChainState
{
    private readonly Action<string> _update;

    internal IncomeDescription(Action<string> update)
    {
        _update = update?? throw new ArgumentNullException(nameof(update));
    }
    
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(chatId, "Enter the description", cancellationToken:cancellationToken);
    }

    public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        _update(message.Text);
        return ChainStatus.Success();
    }
}