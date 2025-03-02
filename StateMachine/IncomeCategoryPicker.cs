using Domain;
using Infrastructure;
using Infrastructure.Telegram;

namespace StateMachine;

internal class IncomeCategoryPicker : IChainState
{
    private readonly Action<IncomeCategory> _update;
    private readonly IEnumerable<IncomeCategory> _categories;

    internal IncomeCategoryPicker(Action<IncomeCategory> update, IEnumerable<IncomeCategory> categories)
    {
        _update = update;
        _categories = categories;
    }
    
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            new EditableMessageToSend()
            {
                ChatId = chatId, 
                Text = "Enter the category", 
                Keyboard = TelegramKeyboard.FromButtons(_categories.Select(c => new TelegramButton() { Text = c.Name, CallbackData = c.Name })), 
            }, cancellationToken: cancellationToken);
    }

    public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        var chosenCategory = _categories.FirstOrDefault(c => string.Equals(c.Name, message.Text));

        if (chosenCategory != null)
        {
            _update(chosenCategory);

            return ChainStatus.Success();
        }

        return ChainStatus.Retry(this);
    }
}