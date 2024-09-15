using Domain;
using Infrastructure;
using Infrastructure.Telegram;

namespace StateMachine;

internal class CurrencyPicker : IChainState
{
    private readonly FilterUpdateStrategy<Currency> _update;

    public CurrencyPicker(FilterUpdateStrategy<Currency> update)
    {
        _update = update;
    }

    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        var keyboard = TelegramKeyboard.FromButtons(new[]
        {
            new TelegramButton{Text = "All", CallbackData = "All"},
            new TelegramButton{Text = Currency.Rur.Name, CallbackData = Currency.Rur.Symbol},
            new TelegramButton{Text = Currency.Amd.Name, CallbackData = Currency.Amd.Symbol},
            new TelegramButton{Text = Currency.Gel.Name, CallbackData = Currency.Gel.Symbol},
        });
        
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Enter the currency",
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        if (Currency.TryParse(message.Text, out var currency))
        {
            _update.Update(currency);
            return ChainStatus.Success();
        }

        // treat like all currencies have been requested
        return ChainStatus.Success();
    }
}