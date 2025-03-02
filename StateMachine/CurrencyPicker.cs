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
            new TelegramButton{Text = Currency.Rur.Name, CallbackData = Currency.Rur.Symbol},
            new TelegramButton{Text = Currency.Amd.Name, CallbackData = Currency.Amd.Symbol},
            new TelegramButton{Text = Currency.Gel.Name, CallbackData = Currency.Gel.Symbol},
            new TelegramButton{Text = Currency.USD.Name, CallbackData = Currency.USD.Symbol},
            new TelegramButton{Text = "All", CallbackData = "All"},
        });
        
        return await botClient.SendTextMessageAsync(
            new EditableMessageToSend(){ChatId = chatId, Text = "Enter the currency", Keyboard = keyboard},
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