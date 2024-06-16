using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class EnterPriceState : IExpenseInfoState
    {
        private readonly ExpenseBuilder _expenseBuilder;
        private readonly IMoneyParser _moneyParser;
        private readonly ILogger _logger;
    
        public IExpenseInfoState PreviousState { get; private set; }

        internal EnterPriceState(IExpenseInfoState previousState, ExpenseBuilder expenseBuilder, IMoneyParser moneyParser, ILogger logger)
        {
            _expenseBuilder = expenseBuilder;
            _moneyParser = moneyParser;
            _logger = logger;
            PreviousState = previousState;
        }

        public bool UserAnswerIsRequired => true;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(chatId, "Enter the price", cancellationToken: cancellationToken);
        }

        public async Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (_moneyParser.TryParse(message.Text, out var money))
                {
                    _expenseBuilder.Sum = money;
                }
            });
        }

        // TODO side effect
        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            if (_expenseBuilder.Sum == null)
            {
                string warning = $"{message.Text} wasn't recognized as money.";
                _logger.LogDebug(warning);
            
                return stateFactory.CreateErrorWithRetryState(warning, this);
            }

            return stateFactory.CreateConfirmState(_expenseBuilder.Build(), this);
        }
    }
}