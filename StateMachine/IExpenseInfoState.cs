using Infrastructure;

namespace StateMachine
{
    public interface IExpenseInfoState
    {
        bool UserAnswerIsRequired { get; }
        IExpenseInfoState PreviousState { get; }
        Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken);
        IExpenseInfoState Handle(string text, CancellationToken cancellationToken);
    }
}