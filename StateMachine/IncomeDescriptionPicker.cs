using Infrastructure;

namespace StateMachine;

class DescriptionPicker : IChainState
{
    private readonly Action<string> _update;

    internal DescriptionPicker(Action<string> update)
    {
        _update = update?? throw new ArgumentNullException(nameof(update));
    }
    
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(chatId, "Write a description", cancellationToken:cancellationToken);
    }

    public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        _update(message.Text);
        return ChainStatus.Success();
    }
}