namespace Infrastructure;

public class ChainStatus
{
    protected ChainStatus()
    {
        
    }

    public virtual bool CanMoveNext { get; }
    public virtual IChainState? State { get; }

    public static ChainStatus Success() => new SuccessChainStatus();
    public static ChainStatus Retry(IChainState state) => new RetryChainStatus(state);
}