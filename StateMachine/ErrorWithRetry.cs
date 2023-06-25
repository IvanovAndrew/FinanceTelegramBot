using Infrastructure;

namespace StateMachine
{
    internal class ErrorWithRetry : IExpenseInfoState
    {
        private readonly string _errorMessage;
        private readonly IExpenseInfoState _state;

        public IExpenseInfoState PreviousState => _state.PreviousState;
    
        internal ErrorWithRetry(string message, IExpenseInfoState state)
        {
            _errorMessage = message;
            _state = state;
        }

        public bool UserAnswerIsRequired => _state.UserAnswerIsRequired;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(chatId, _errorMessage, cancellationToken: cancellationToken);

            return await _state.Request(botClient, chatId, cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            return _state.Handle(text, cancellationToken);
        }
    }
}