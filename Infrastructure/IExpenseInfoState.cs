using Infrastructure;

namespace Infrastructure
{
    public interface IExpenseInfoState
    {
        bool UserAnswerIsRequired { get; }
        
        Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default);

        Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            if (!TelegramCommand.TryGetCommand(message.Text, out _))
            {
                return HandleInternal(message, cancellationToken);
            }
            
            return Task.CompletedTask;
        }

        IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory);
        IExpenseInfoState MoveToNextState(IMessage message, IStateFactory stateFactory, CancellationToken cancellationToken)
        {
            if (TelegramCommand.TryGetCommand(message.Text, out var command))
            {
                return command.Execute(this, stateFactory);
            }

            return ToNextState(message, stateFactory, cancellationToken);
        }

        protected IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken);

        protected Task HandleInternal(IMessage message, CancellationToken cancellationToken);
    }

    public interface IChainState
    {
        Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default);
        ChainStatus HandleInternal(IMessage message, CancellationToken cancellationToken);
    }
}

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

internal class SuccessChainStatus : ChainStatus
{
    public override bool CanMoveNext => true;
    public override IChainState? State => default;
}

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
