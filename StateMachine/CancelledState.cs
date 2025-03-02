using Infrastructure;
using Infrastructure.Telegram;
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

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ChatId = chatId, Text = "Canceled"});
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