using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class EnterTheWayState : IExpenseInfoState
{
    public bool UserAnswerIsRequired => true;
    public IExpenseInfoState PreviousState { get; }
    private readonly ILogger _logger;
        
    internal EnterTheWayState(IExpenseInfoState previousState, ILogger logger)
    {
        _logger = logger;
        PreviousState = previousState;
    }
        
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        var keyboard = TelegramKeyboard.FromButtons(new[]
        {
            new TelegramButton{Text = "By myself", CallbackData = "user"},
            new TelegramButton{Text = "From check", CallbackData = "json"},
            new TelegramButton{Text = "From qr", CallbackData = "rawqr"},
        });
        
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Enter the expense",
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    public Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        if (message.Text == "json") return stateFactory.CreateRequestPasteJsonState(this);
        if (message.Text == "user") return stateFactory.CreateEnterTheDateState(this, false);
        if (message.Text == "rawqr") return stateFactory.CreateEnterRawQrState(this);

        throw new BotStateException(new []{"json", "user", "rawqr"}, message.Text);
    }
}