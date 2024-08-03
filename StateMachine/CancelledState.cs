using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class CancelledState : IExpenseInfoState
    {
        private readonly ILogger _logger;

        public CancelledState(ILogger logger)
        {
            _logger = logger;
        }

        public bool UserAnswerIsRequired => false;

        public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return botClient.SendTextMessageAsync(chatId, "Canceled");
        }

        public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            return stateFactory.CreateGreetingState();
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            return stateFactory.CreateGreetingState();
        }
    }
}