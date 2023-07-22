using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class CancelledState : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly ILogger _logger;
    
        public IExpenseInfoState PreviousState { get; private set; }

        public CancelledState(StateFactory factory, ILogger logger)
        {
            _factory = factory;
            _logger = logger;
        }
    
        public bool UserAnswerIsRequired => false;
        public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return botClient.SendTextMessageAsync(chatId, "Canceled");
        }

        public async Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Run(() => { });
        }

        public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}