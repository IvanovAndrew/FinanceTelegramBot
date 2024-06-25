using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class EnterTheWayState : IExpenseInfoState
{
    public bool UserAnswerIsRequired => true;
    private readonly ILogger _logger;
        
    internal EnterTheWayState(ILogger logger)
    {
        _logger = logger;
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

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        return stateFactory.CreateGreetingState();
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        if (message.Text == "json") return stateFactory.CreateRequestPasteJsonState();
        if (message.Text == "user") return stateFactory.CreateEnterTheDateState(this, false);
        if (message.Text == "rawqr") return stateFactory.CreateEnterRawQrState();

        throw new BotStateException(new []{"json", "user", "rawqr"}, message.Text);
    }
}