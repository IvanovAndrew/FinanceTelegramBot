using Infrastructure;

namespace StateMachine;

public class CommandAttribute : Attribute
{
    public string Text { get; init; }
    public string Command { get; init; }
}

public abstract class TelegramCommand
{
    protected readonly ITelegramBot _telegramBot;
    protected readonly StateFactory _stateFactory;
    protected readonly long _chatId;
    protected TelegramCommand(ITelegramBot telegramBot, StateFactory stateFactory, long chatId)
    {
        _telegramBot = telegramBot;
        _stateFactory = stateFactory;
        _chatId = chatId;
    }
    
    public abstract Task<IExpenseInfoState> Execute(IExpenseInfoState state, CancellationTokenSource cancellationTokenSource = default);
}

[Command(Text = "Cancel", Command = "/cancel")]
public class CancelCommand : TelegramCommand
{
    public CancelCommand(ITelegramBot telegramBot, StateFactory stateFactory, long chatId) : base(telegramBot, stateFactory, chatId)
    {
    }
    
    public override async Task<IExpenseInfoState> Execute(IExpenseInfoState state, CancellationTokenSource cancellationTokenSource = default)
    {
        if (state is ILongTermOperation longTermOperation)
        {
            longTermOperation.Cancel();
        }
        
        await _telegramBot.SendTextMessageAsync(_chatId, "All operations are canceled");

        return _stateFactory.CreateGreetingState();
    }

}

[Command(Text = "Start", Command = "/start")]
public class StartCommand : TelegramCommand
{
    public StartCommand(ITelegramBot telegramBot, StateFactory stateFactory, long chatId) : base(telegramBot, stateFactory, chatId)
    {
    }
    
    public override Task<IExpenseInfoState> Execute(IExpenseInfoState state, CancellationTokenSource cancellationTokenSource)
    {
        return Task.FromResult(_stateFactory.CreateGreetingState());
    }
}

[Command(Text = "Back", Command = "/back")]
public class BackCommand : TelegramCommand
{
    public BackCommand(ITelegramBot telegramBot, StateFactory stateFactory, long chatId) : base(telegramBot, stateFactory, chatId)
    {
    }
    
    public override Task<IExpenseInfoState> Execute(IExpenseInfoState state, CancellationTokenSource cancellationTokenSource)
    {
        return Task.FromResult(state.PreviousState);
    }
}

