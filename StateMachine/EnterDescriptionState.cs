using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class EnterDescriptionState : IExpenseInfoState
    {
        private readonly ExpenseBuilder _expenseBuilder;
        private readonly ILogger _logger;
    
        internal EnterDescriptionState(ExpenseBuilder expenseBuilder, ILogger logger)
        {
            _expenseBuilder = expenseBuilder;
            _logger = logger;
        }

        public bool UserAnswerIsRequired => true;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(chatId, "Write a description", cancellationToken: cancellationToken);
        }

        public async Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Run(() => _expenseBuilder.Description = message.Text);
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            if (_expenseBuilder.SubCategory != null)
            {
                return stateFactory.CreateEnterTheSubcategoryState(_expenseBuilder, _expenseBuilder.Category!.Subcategories);
            }
            else
            {
                return stateFactory.CreateEnterTheCategoryState(_expenseBuilder);
            }
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return stateFactory.CreateEnterThePriceState(_expenseBuilder);
        }
    }
}