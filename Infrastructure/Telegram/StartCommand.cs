namespace Infrastructure.Telegram;

[Command(Text = "Start", Command = "/start", Order = 0)]
public class StartCommand : TelegramCommand
{
    protected override IExpenseInfoState ToNextState(IExpenseInfoState state, IStateFactory stateFactory)
    {
        return stateFactory.CreateGreetingState();
    }
}