namespace StateMachine;

internal class InsideChainStatus : MoveStatus
{
    internal override bool IsOutOfChain => false;
}