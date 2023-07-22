using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class EnterTheWayState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    public bool UserAnswerIsRequired => true;
    public IExpenseInfoState PreviousState { get; }
    private readonly ILogger _logger;
        
    internal EnterTheWayState(StateFactory factory, IExpenseInfoState previousState, ILogger logger)
    {
        _factory = factory;
        _logger = logger;
        PreviousState = previousState;
    }
        
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        var keyboard = TelegramKeyboard.FromButtons(new[]
        {
            new TelegramButton{Text = "By myself", CallbackData = "user"},
            new TelegramButton{Text = "From json", CallbackData = "json"},
        });
        
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Enter the expense",
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    public async Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        await Task.Run(() => { });
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
    {
        if (message.Text == "json") return _factory.CreateRequestPasteJsonState(this);
        if (message.Text == "user") return _factory.CreateEnterTheDateState(this, false);

        throw new BotStateException(new []{"json", "user"}, message.Text);
    }
}