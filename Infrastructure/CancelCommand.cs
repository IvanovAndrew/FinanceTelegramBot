using StateMachine;

namespace Infrastructure;

[Command(Text = "Cancel", Command = "/cancel", Order = 2)]
public class CancelCommand : TelegramCommand
{
    protected override IExpenseInfoState ToNextState(IExpenseInfoState state, IStateFactory stateFactory)
    {
        return stateFactory.CreateGreetingState();
    }
}