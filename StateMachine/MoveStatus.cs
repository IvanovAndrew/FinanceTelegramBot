namespace StateMachine;

internal abstract class MoveStatus
{
    internal abstract bool IsOutOfChain { get; }
    
    internal static MoveStatus InsideChainStatus() => new InsideChainStatus();
    internal static MoveStatus OutOfChainStatus() => new OutOfChainStatus();
}