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
            new TelegramButton{Text = "From check", CallbackData = "check"},
        });
        
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Enter the expense",
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
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
        if (message.Text == "user") return stateFactory.CreateEnterTheDateState(this, false);
        if (message.Text == "check") return stateFactory.CreateCheckInfoState();

        throw new BotStateException(new []{"user", "check"}, message.Text);
    }
}