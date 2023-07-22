namespace Infrastructure
{
    public interface IExpenseInfoState
    {
        bool UserAnswerIsRequired { get; }
        IExpenseInfoState PreviousState { get; }
        Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default);
        Task Handle(IMessage message, CancellationToken cancellationToken);
        
        IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken);
    }
}