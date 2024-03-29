using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class EnterDescriptionState : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly ExpenseBuilder _expenseBuilder;
        private readonly ILogger _logger;
    
        public IExpenseInfoState PreviousState { get; private set; }
    
        internal EnterDescriptionState(StateFactory factory, IExpenseInfoState previousState, ExpenseBuilder expenseBuilder, ILogger logger)
        {
            _factory = factory;
            _expenseBuilder = expenseBuilder;
            _logger = logger;

            PreviousState = previousState;
        }

        public bool UserAnswerIsRequired => true;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(chatId, "Write a description", cancellationToken: cancellationToken);
        }

        public async Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Run(() => _expenseBuilder.Description = message.Text);
        }

        public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _factory.CreateEnterThePriceState(_expenseBuilder, this);
        }
    }
}