using Infrastructure;

namespace StateMachine;

public class InitialState : IExpenseInfoState
{
    public bool UserAnswerIsRequired => false;

    public InitialState()
    {
    }

    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        throw new MissingInformationBotException();
    }

    public Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) => this;

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