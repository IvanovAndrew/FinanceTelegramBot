namespace Infrastructure;

internal class SuccessChainStatus : ChainStatus
{
    public override bool CanMoveNext => true;
    public override IChainState? State => default;
}