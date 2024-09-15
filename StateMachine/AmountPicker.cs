using Domain;
using Infrastructure;
using Infrastructure.Telegram;

namespace StateMachine;

class AmountPicker : IChainState
{
    private readonly Action<Money> _update;
    private readonly string _title;

    internal AmountPicker(Action<Money> update, string title)
    {
        _update = update ?? throw new ArgumentNullException(nameof(update));
        _title = title;
    }

    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(chatId, $"Enter the {_title}", cancellationToken: cancellationToken);
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