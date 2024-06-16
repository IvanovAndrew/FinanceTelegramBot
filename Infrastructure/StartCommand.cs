using StateMachine;

namespace Infrastructure;

[Command(Text = "Start", Command = "/start", Order = 0)]
public class StartCommand : TelegramCommand
{
    public override IExpenseInfoState Execute(IExpenseInfoState state, IStateFactory stateFactory)
    {
        return stateFactory.CreateGreetingState();
    }
}