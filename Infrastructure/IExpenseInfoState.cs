using Application;
using Infrastructure.Telegram;

namespace Infrastructure
{
    public interface IExpenseInfoState
    {
        bool UserAnswerIsRequired { get; }
        
        Task<IMessage> Request(IMessageService botClient, long chatId, CancellationToken cancellationToken = default);

        Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            
            
            return Task.CompletedTask;
        }

        IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory);
        IExpenseInfoState MoveToNextState(IMessage message, IStateFactory stateFactory, CancellationToken cancellationToken)
        {
            
            return ToNextState(message, stateFactory, cancellationToken);
        }

        protected IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken);

        protected Task HandleInternal(IMessage message, CancellationToken cancellationToken);
    }
}