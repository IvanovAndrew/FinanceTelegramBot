using Infrastructure;

namespace StateMachine;

public class InitialState : IExpenseInfoState
{
    public bool UserAnswerIsRequired => false;
    public IExpenseInfoState PreviousState => this;

    public InitialState()
    {
    }

    public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        if (TelegramCommand.TryGetCommand(message.Text, out var command))
        {
            return command.Execute(this, stateFactory);
        }

        return this;
    }
}