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
    
        internal SaveExpenseState(IExpense expense, IExpenseRepository expenseRepository, ILogger logger)
        {
            _expense = expense;
            _expenseRepository = expenseRepository;
            _logger = logger;
        }

        public bool UserAnswerIsRequired => false;

        public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        public Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            if (TelegramCommand.TryGetCommand(message.Text, out _))
            {
                Cancel();
            }
            
            return Task.CompletedTask;
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            Cancel();
            return stateFactory.CreateConfirmState(_expense);
        }

        public async Task<IMessage> Handle(ITelegramBot botClient, IMessage message, CancellationToken cancellationToken)
        {
            var savingMessage = await botClient.SendTextMessageAsync(message.ChatId, "Saving... It can take some time.");

            SaveExpenseResult result;
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    var saved = await _expenseRepository.Save(_expense, _cancellationTokenSource.Token);
                    result = saved ? SaveExpenseResult.Saved(_expense) : SaveExpenseResult.Failed(_expense, "");
                }
            }
            catch (OperationCanceledException)
            {
                result = SaveExpenseResult.Canceled(_expense);
                _logger.LogInformation("Operation is canceled by user");
            }
            catch(Exception e)
            {
                result = SaveExpenseResult.Failed(_expense, e.Message);
                _logger.LogInformation($"Couldn't save an expense: {e}");
            }
            finally
            {
                _cancellationTokenSource = null;
            }

            await botClient.DeleteMessageAsync(savingMessage, cancellationToken);
            return await botClient.SendTextMessageAsync(message.ChatId, result.GetMessage());
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            return stateFactory.CreateGreetingState();
        }

        public Task Cancel()
        {
            return Task.Run(
                () =>
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                });
        }
    }
}