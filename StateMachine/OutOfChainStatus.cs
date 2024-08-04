namespace StateMachine;

internal class OutOfChainStatus : MoveStatus
{
    internal override bool IsOutOfChain => true;
}