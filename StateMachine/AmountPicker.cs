using Domain;
using Infrastructure;

namespace StateMachine;

class AmountPicker : IChainState
{
    private readonly Action<Money> _update;

    internal AmountPicker(Action<Money> update)
    {
        _update = update ?? throw new ArgumentNullException(nameof(update));
    }

    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(chatId, "Enter the income", cancellationToken: cancellationToken);
    }

    public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        if (Money.TryParse(message.Text, out var money))
        {
            _update(money);
            return ChainStatus.Success();
        }
        
        return ChainStatus.Retry(this);
    }
}