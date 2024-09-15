using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class DatePickerState : IChainState
{
    private readonly UpdateStrategy<DateOnly> _update;
    protected readonly string Text;
    protected readonly DateOnly Today;
    protected readonly string DateFormat;
    private readonly Dictionary<DateOnly, string> _options;
    private readonly string _customOptionTitle;
    private readonly ILogger _logger;
    private const string CallbackCustom = "custom";

    internal DatePickerState(UpdateStrategy<DateOnly> update, string text, DateOnly today, string dateFormat, Dictionary<DateOnly, string> options, string customOptionTitle, ILogger logger) : this(update, text, today, dateFormat, logger)
    {
        _update = update;
        Text = text;
        DateFormat = dateFormat;
        _options = options;
        _customOptionTitle = customOptionTitle;
    }
    
    protected DatePickerState(UpdateStrategy<DateOnly> update, string text, DateOnly today, string dateFormat, ILogger logger)
    {
        _update = update;
        Text = text;
        Today = today;
        DateFormat = dateFormat;
        _options = new Dictionary<DateOnly, string>();
        _customOptionTitle = string.Empty;
        _logger = logger;
    }
    
    public virtual async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        var buttons = new TelegramButton[_options.Count + 1];

        int i = 0;
        foreach (var (date, alias) in _options)
        {
            buttons[i++] = new TelegramButton() { Text = alias, CallbackData = date.ToString(DateFormat) };
        }

        buttons[i] = new TelegramButton { Text = _customOptionTitle, CallbackData = CallbackCustom };

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
            return ChainStatus.Retry(new CustomDatePickerState(_update, Text, Today, DateFormat, _logger));
        }

        _logger.LogWarning($"Couldn't parse {message.Text} as a date");
        
        return ChainStatus.Retry(this);
    }
}

internal class CustomDatePickerState : DatePickerState
{
    protected internal CustomDatePickerState(UpdateStrategy<DateOnly> update, string text, DateOnly today, string dateFormat, ILogger logger) : base(update, text, today, dateFormat, logger)
    {
    }

    public override async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId, $"{Text}. Example: {Today.ToString(DateFormat)}", cancellationToken:cancellationToken);
    }
}