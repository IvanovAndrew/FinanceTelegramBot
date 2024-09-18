using Infrastructure;

internal class RetryChainStatus : ChainStatus
{
    private readonly IChainState _state;
    internal RetryChainStatus(IChainState state)
    {
        _state = state;
    }
    public override bool CanMoveNext => false;
    public override IChainState State => _state;
}