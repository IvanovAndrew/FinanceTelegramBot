using Infrastructure;

namespace StateMachine;

internal class StateChain
{
    private readonly IExpenseInfoState _originalState;
    private readonly IChainState[] _chain;
    private int Current = 0;
    private ChainStatus _chainStatus;

    internal StateChain(IExpenseInfoState originalState, params IChainState[] chain)
    {
        _originalState = originalState;
        _chain = chain;
    }

    public bool UserAnswerIsRequired { get; }

    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await _chain[Current].Request(botClient, chatId, cancellationToken);
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        if (Current > 0)
        {
            Current--;
            return null;
        }

        return _originalState;
    }

    public IExpenseInfoState? ToNextState()
    {
        if (_chainStatus.CanMoveNext)
        {
            Current++;
        }
        else
        {
            _chain[Current] = _chainStatus.State!;
        }
        
        if (Current < _chain.Length)
        {
            return null;
        }

        return _originalState;
    }

    public void Handle(IMessage message, CancellationToken cancellationToken)
    {
        _chainStatus = _chain[Current].HandleInternal(message, cancellationToken);
    }
}