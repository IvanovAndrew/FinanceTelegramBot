using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class EnterPriceState : IExpenseInfoState
    {
        private readonly ExpenseBuilder _expenseBuilder;
        private readonly ILogger _logger;
    
        internal EnterPriceState(ExpenseBuilder expenseBuilder, ILogger logger)
        {
            _expenseBuilder = expenseBuilder;
            _logger = logger;
        }

        public bool UserAnswerIsRequired => true;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(chatId, "Enter the price", cancellationToken: cancellationToken);
        }

        public async Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (Money.TryParse(message.Text, out var money))
                {
                    _expenseBuilder.Sum = money;
                }
            });
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) =>stateFactory.CreateEnterDescriptionState(_expenseBuilder);

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            if (_expenseBuilder.Sum == null)
            {
                string warning = $"{message.Text} wasn't recognized as money.";
                _logger.LogDebug(warning);
            
                return stateFactory.CreateErrorWithRetryState(warning, this);
            }

            return stateFactory.CreateConfirmState(_expenseBuilder.Build());
        }
    }
}