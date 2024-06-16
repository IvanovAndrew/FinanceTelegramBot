using StateMachine;

namespace Infrastructure;

[Command(Text = "Cancel", Command = "/cancel", Order = 2)]
public class CancelCommand : TelegramCommand
{
    public override IExpenseInfoState Execute(IExpenseInfoState state, IStateFactory stateFactory)
    {
        if (state is ILongTermOperation longTermOperation)
        {
            longTermOperation.Cancel();
        }

        return stateFactory.CreateGreetingState();
    }

}