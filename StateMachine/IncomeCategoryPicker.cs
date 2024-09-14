using Domain;
using Infrastructure;

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
        string infoMessage = "Enter the category";

        // keyboard
        var keyboard = TelegramKeyboard.FromButtons(_categories.Select(c => new TelegramButton()
            { Text = c.Name, CallbackData = c.Name })); 
        
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: infoMessage,
            keyboard: keyboard,
            cancellationToken: cancellationToken);
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