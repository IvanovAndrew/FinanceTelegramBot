using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class SaveExpenseState : IExpenseInfoState, ILongTermOperation
    {
        private readonly StateFactory _factory;
        private readonly IExpense _expense;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ILogger _logger;
        private CancellationTokenSource? _cancellationTokenSource;
    
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
            var message = await botClient.SendTextMessageAsync(chatId, "Saving... It can take some time.");

            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                await _expenseRepository.Save(_expense, cancellationToken);
            }

            _cancellationTokenSource = null;

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

        public IExpenseInfoState Handle(IMessage message, CancellationToken cancellationToken)
        {
            return _factory.CreateGreetingState();
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
    
    internal class SaveExpensesFromJsonState : IExpenseInfoState, ILongTermOperation
    {
        private readonly StateFactory _factory;
        private readonly List<IExpense> _expenses;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ILogger _logger;
        private CancellationTokenSource? _cancellationTokenSource;
    
        public IExpenseInfoState PreviousState { get; private set; }
    
        internal SaveExpensesFromJsonState(StateFactory factory, IExpenseInfoState previousState, List<IExpense> expenses, IExpenseRepository expenseRepository, ILogger logger)
        {
            _factory = factory;
            _expenses = expenses;
            _expenseRepository = expenseRepository;
            _logger = logger;

            PreviousState = previousState;
        }

        public bool UserAnswerIsRequired => false;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            var message = await botClient.SendTextMessageAsync(chatId, "Saving... It can take some time.");

            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                foreach (var expense in _expenses)
                {
                    await _expenseRepository.Save(expense, cancellationToken);
                }
            }

            _cancellationTokenSource = null;

            var sum = new Money() { Amount = 0, Currency = _expenses[0].Amount.Currency };
            foreach (var expense in _expenses)
            {
                sum += expense.Amount;
            }

            string infoMessage = $"All expenses are saved with the following options: {Environment.NewLine}" + 
                string.Join($"{Environment.NewLine}", 
                    $"Date: {_expenses[0].Date:dd.MM.yyyy}", 
                    $"Category: {_expenses[0].Category}", 
                    $"Total Amount: {sum}",
                    "",
                "Saved"
            );
            
            _logger.LogInformation(infoMessage);

            await botClient.DeleteMessageAsync(chatId, message.Id, cancellationToken);
            return await botClient.SendTextMessageAsync(chatId, infoMessage);
        }

        public IExpenseInfoState Handle(IMessage message, CancellationToken cancellationToken)
        {
            return _factory.CreateGreetingState();
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}