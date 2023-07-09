using Infrastructure;

namespace StateMachine
{
    class CancelExpenseState : IExpenseInfoState
    {
        public IExpenseInfoState PreviousState { get; private set; }
        
        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(chatId, "Operation is canceled", cancellationToken: cancellationToken);
        }

        public bool UserAnswerIsRequired => false;
        public IExpenseInfoState Handle(IMessage message, CancellationToken cancellationToken)
        { 
            throw new InvalidOperationException();
        }
    }
}