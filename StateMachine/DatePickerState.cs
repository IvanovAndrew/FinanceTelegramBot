﻿using Infrastructure;

namespace StateMachine;

internal class DatePickerState : IExpenseInfoState
{
    protected readonly string Text;
    protected readonly DateOnly Today;
    protected readonly string DateFormat;
    private readonly DateOnly[] _options;
    private readonly string _customOptionTitle;
    private const string CallbackCustom = "custom";

    internal DatePickerState(IExpenseInfoState previousState, string text, DateOnly today, string dateFormat, DateOnly[] options, string customOptionTitle) : this(text, today, dateFormat, previousState)
    {
        Text = text;
        DateFormat = dateFormat;
        _options = options;
        _customOptionTitle = customOptionTitle;
    }
    
    protected DatePickerState(string text, DateOnly today, string dateFormat, IExpenseInfoState previousState)
    {
        Text = text;
        Today = today;
        DateFormat = dateFormat;
        _options = Array.Empty<DateOnly>();
        _customOptionTitle = string.Empty;
        PreviousState = previousState;
    }
    
    public bool UserAnswerIsRequired => true;
    public IExpenseInfoState PreviousState { get; }

    
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

    public Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
    {
        if (message.Text == CallbackCustom)
        {
            return new CustomDatePickerState(Text, Today, DateFormat, PreviousState);
        }

        return PreviousState;
    }
}

internal class CustomDatePickerState : DatePickerState
{
    protected internal CustomDatePickerState(string text, DateOnly today, string dateFormat, IExpenseInfoState previousState) : base(text, today, dateFormat, previousState)
    {
    }

    public override async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId, $"{Text}. Example: {Today.ToString(DateFormat)}", cancellationToken:cancellationToken);
    }
}