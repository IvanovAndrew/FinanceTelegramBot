using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class SaveExpenseState : IExpenseInfoState, ILongTermOperation
    {
        private readonly IExpense _expense;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ILogger _logger;
        private CancellationTokenSource? _cancellationTokenSource;
    
        public IExpenseInfoState PreviousState { get; private set; }
    
        internal SaveExpenseState(IExpenseInfoState previousState, IExpense expense, IExpenseRepository expenseRepository, ILogger logger)
        {
            _expense = expense;
            _expenseRepository = expenseRepository;
            _logger = logger;

            PreviousState = previousState;
        }

        public bool UserAnswerIsRequired => false;

        public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        public Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IMessage> Handle(ITelegramBot botClient, IMessage message, CancellationToken cancellationToken)
        {
            var savingMessage = await botClient.SendTextMessageAsync(message.ChatId, "Saving... It can take some time.");
            bool saved = false;
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    await _expenseRepository.Save(_expense, _cancellationTokenSource.Token);
                }

                saved = true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation is canceled by user");
            }
            finally
            {
                _cancellationTokenSource = null;
            }

            string infoMessage = string.Join($"{Environment.NewLine}", 
                    $"Date: {_expense.Date:dd.MM.yyyy}", 
                    $"Category: {_expense.Category}", 
                    $"Subcategory: {_expense.SubCategory ?? string.Empty}", 
                    $"Description: {_expense.Description ?? string.Empty}",
                    $"Amount: {_expense.Amount}",
                    "",
                    saved? "Saved" : "Saving is canceled"
                );

            await botClient.DeleteMessageAsync(savingMessage, cancellationToken);

            _logger.LogInformation(infoMessage);
            return await botClient.SendTextMessageAsync(message.ChatId, infoMessage);
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            return stateFactory.CreateGreetingState();
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}