using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class CancelledState : IExpenseInfoState
    {
        private readonly ILogger _logger;

        public IExpenseInfoState PreviousState { get; private set; }

        public CancelledState(ILogger logger)
        {
            _logger = logger;
        }

        public bool UserAnswerIsRequired => false;

        public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return botClient.SendTextMessageAsync(chatId, "Canceled");
        }

        public Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}