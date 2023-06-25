using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;
using TelegramBot;

namespace StateMachine
{
    internal class SaveExpenseState : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly IExpense _expense;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ILogger _logger;
    
        public IExpenseInfoState PreviousState { get; private set; }
    
        internal SaveExpenseState(StateFactory factory, IExpenseInfoState previousState, IExpense expense, IExpenseRepository expenseRepository, ILogger logger)
        {
            _factory = factory;
            _expense = expense;
            _expenseRepository = expenseRepository;
            _logger = logger;

            PreviousState = previousState;
        }

        public bool UserAnswerIsRequired => false;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Saving... It can take some time.");
            var message = await botClient.SendTextMessageAsync(chatId, "Saving... It can take some time.");
            await _expenseRepository.Save(_expense, cancellationToken);

            string infoMessage = string.Join($"{Environment.NewLine}", 
                $"Date: {_expense.Date:dd.MM.yyyy}", 
                $"Category: {_expense.Category}", 
                $"SubCategory: {_expense.SubCategory ?? string.Empty}", 
                $"Description: {_expense.Description ?? string.Empty}",
                $"Amount: {_expense.Amount}",
                "",
                "Saved"
            );
            
            _logger.LogInformation(infoMessage);

            await botClient.DeleteMessageAsync(chatId, message.Id, cancellationToken);
            return await botClient.SendTextMessageAsync(chatId, infoMessage);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }
    }
}