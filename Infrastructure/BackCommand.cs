using StateMachine;

namespace Infrastructure;

[Command(Text = "Back", Command = "/back", Order = 1)]
public class BackCommand : TelegramCommand
{
    protected override IExpenseInfoState ToNextState(IExpenseInfoState state, IStateFactory stateFactory)
    {
        return state.MoveToPreviousState(stateFactory);
    }
}