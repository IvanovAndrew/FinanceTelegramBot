using StateMachine;

namespace Infrastructure;

[Command(Text = "Back", Command = "/back", Order = 1)]
public class BackCommand : TelegramCommand
{
    public override IExpenseInfoState Execute(IExpenseInfoState state, IStateFactory stateFactory)
    {
        return state.PreviousState;
    }
}