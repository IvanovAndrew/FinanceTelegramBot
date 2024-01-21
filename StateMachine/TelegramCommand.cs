using Infrastructure;

namespace StateMachine;

public class CommandAttribute : Attribute
{
    public string Text { get; init; } = "";
    public string Command { get; init; } = "";
}

public abstract class TelegramCommand
{
    protected readonly StateFactory StateFactory;
    protected readonly long ChatId;
    protected TelegramCommand(StateFactory stateFactory, long chatId)
    {
        StateFactory = stateFactory;
        ChatId = chatId;
    }
    
    public abstract Task<IExpenseInfoState> Execute(IExpenseInfoState state, CancellationToken cancellationToken = default);
}

[Command(Text = "Cancel", Command = "/cancel")]
public class CancelCommand : TelegramCommand
{
    public CancelCommand(StateFactory stateFactory, long chatId) : base(stateFactory, chatId)
    {
    }
    
    public override Task<IExpenseInfoState> Execute(IExpenseInfoState state, CancellationToken cancellationToken = default)
    {
        if (state is ILongTermOperation longTermOperation)
        {
            longTermOperation.Cancel();
        }

        return Task.FromResult(StateFactory.CreateGreetingState());
    }

}

[Command(Text = "Start", Command = "/start")]
public class StartCommand : TelegramCommand
{
    public StartCommand(StateFactory stateFactory, long chatId) : base(stateFactory, chatId)
    {
    }
    
    public override Task<IExpenseInfoState> Execute(IExpenseInfoState state, CancellationToken cancellationTokenSource = default)
    {
        return Task.FromResult(StateFactory.CreateGreetingState());
    }
}

[Command(Text = "Back", Command = "/back")]
public class BackCommand : TelegramCommand
{
    public BackCommand(StateFactory stateFactory, long chatId) : base(stateFactory, chatId)
    {
    }
    
    public override Task<IExpenseInfoState> Execute(IExpenseInfoState state, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(state.PreviousState);
    }
}

