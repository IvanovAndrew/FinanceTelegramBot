using Infrastructure;

namespace StateMachine;

abstract class StateWithChainsBase : IExpenseInfoState
{
    protected StateChain StateChain { get; set; }
    
    public bool UserAnswerIsRequired => true;
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await StateChain.Request(botClient, chatId, cancellationToken);
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        return StateChain.MoveToPreviousState().IsOutOfChain? PreviousState(stateFactory): this;
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory, CancellationToken cancellationToken)
    {
        return StateChain.MoveToNextState().IsOutOfChain ? NextState(stateFactory) : this;
    }
    
    protected abstract IExpenseInfoState PreviousState(IStateFactory stateFactory);
    protected abstract IExpenseInfoState NextState(IStateFactory stateFactory);

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        StateChain.Handle(message, cancellationToken);
        return Task.CompletedTask;
    }
}