using Infrastructure;
using Infrastructure.Telegram;

namespace StateMachine;

internal class DatePickerState : IChainState
{
    private readonly FilterUpdateStrategy<DateOnly> _update;
    protected readonly string Text;
    protected readonly DateOnly Today;
    protected readonly string DateFormat;
    private readonly DateOnly[] _options;
    private readonly string _customOptionTitle;
    private const string CallbackCustom = "custom";

    internal DatePickerState(FilterUpdateStrategy<DateOnly> update, string text, DateOnly today, string dateFormat, DateOnly[] options, string customOptionTitle) : this(update, text, today, dateFormat)
    {
        _update = update;
        Text = text;
        DateFormat = dateFormat;
        _options = options;
        _customOptionTitle = customOptionTitle;
    }
    
    protected DatePickerState(FilterUpdateStrategy<DateOnly> update, string text, DateOnly today, string dateFormat)
    {
        _update = update;
        Text = text;
        Today = today;
        DateFormat = dateFormat;
        _options = Array.Empty<DateOnly>();
        _customOptionTitle = string.Empty;
    }
    
    public virtual async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        var buttons = new TelegramButton[_options.Length + 1];

        for (int i = 0; i < _options.Length; i++)
        {
            var dateString = _options[i].ToString(DateFormat);
            buttons[i] = new TelegramButton() { Text = dateString, CallbackData = dateString };
        }

        buttons[_options.Length] = new TelegramButton { Text = _customOptionTitle, CallbackData = CallbackCustom };

        int chunkSize = 3;
        if (buttons.Length == 4)
        {
            chunkSize = 2;
        }
        
        var keyboard = TelegramKeyboard.FromButtons(buttons, chunkSize: chunkSize);
        
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: Text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    public ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        if (DateOnly.TryParseExact(message.Text, DateFormat, out var selectedMonth))
        {
            _update.Update(selectedMonth);
            return ChainStatus.Success();
        }
        else if (message.Text == CallbackCustom)
        {
            return ChainStatus.Retry(new CustomDatePickerState(_update, Text, Today, DateFormat));
        }

        return ChainStatus.Retry(this);
    }
}

internal class CustomDatePickerState : DatePickerState
{
    protected internal CustomDatePickerState(FilterUpdateStrategy<DateOnly> update, string text, DateOnly today, string dateFormat) : base(update, text, today, dateFormat)
    {
    }

    public override async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId, $"{Text}. Example: {Today.ToString(DateFormat)}", cancellationToken:cancellationToken);
    }
}