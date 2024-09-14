using Infrastructure;

namespace StateMachine;

internal class StateChain
{
    private readonly IChainState[] _chain;
    private int Current = 0;
    private ChainStatus _chainStatus;

    internal StateChain(params IChainState[] chain)
    {
        _chain = chain;
    }

    internal async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await _chain[Current].Request(botClient, chatId, cancellationToken);
    }

    internal MoveStatus MoveToPreviousState()
    {
        Current--;
        return Current >= 0 ? MoveStatus.InsideChainStatus() : MoveStatus.OutOfChainStatus();
    }

    internal MoveStatus MoveToNextState()
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
            return MoveStatus.InsideChainStatus();
        }

        return MoveStatus.OutOfChainStatus();
    }

    internal void Handle(IMessage message, CancellationToken cancellationToken)
    {
        _chainStatus = _chain[Current].HandleInternal(message, cancellationToken);
    }
}