namespace Infrastructure
{
    public interface IExpenseInfoState
    {
        bool UserAnswerIsRequired { get; }
        IExpenseInfoState PreviousState { get; }
        Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default);
        IExpenseInfoState Handle(IMessage message, CancellationToken cancellationToken);
    }

    public interface ILongTermOperation
    {
        void Cancel();
    }
}