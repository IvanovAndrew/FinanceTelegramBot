using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class SaveExpenseState : IExpenseInfoState, ILongTermOperation
    {
        private readonly IExpense _expense;
        private readonly IFinanseRepository _finanseRepository;
        private readonly ILogger _logger;
        private CancellationTokenSource? _cancellationTokenSource;
    
        internal SaveExpenseState(IExpense expense, IFinanseRepository finanseRepository, ILogger logger)
        {
            _expense = expense;
            _finanseRepository = finanseRepository;
            _logger = logger;
        }

        public bool UserAnswerIsRequired => false;

        public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
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

            SaveResult result;
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    var saved = await _finanseRepository.SaveOutcome(_expense, _cancellationTokenSource.Token);
                    result = saved ? SaveResult.Saved(_expense) : SaveResult.Failed(_expense, "");
                }
            }
            catch (OperationCanceledException)
            {
                result = SaveResult.Canceled(_expense);
                _logger.LogInformation("Operation is canceled by user");
            }
            catch(Exception e)
            {
                result = SaveResult.Failed(_expense, e.Message);
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
    
    internal class SaveIncomeState : IExpenseInfoState, ILongTermOperation
    {
        private readonly IIncome _income;
        private readonly IFinanseRepository _finanseRepository;
        private readonly ILogger _logger;
        private CancellationTokenSource? _cancellationTokenSource;
    
        internal SaveIncomeState(IIncome income, IFinanseRepository finanseRepository, ILogger logger)
        {
            _income = income?? throw new ArgumentNullException(nameof(income));
            _finanseRepository = finanseRepository;
            _logger = logger;
        }

        public bool UserAnswerIsRequired => false;

        public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
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
            return stateFactory.CreateConfirmState(_income);
        }

        public async Task<IMessage> Handle(ITelegramBot botClient, IMessage message, CancellationToken cancellationToken)
        {
            var savingMessage = await botClient.SendTextMessageAsync(message.ChatId, "Saving... It can take some time.");

            SaveResult result;
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    var saved = await _finanseRepository.SaveIncome(_income, _cancellationTokenSource.Token);
                    result = saved ? SaveResult.Saved(_income) : SaveResult.Failed(_income, "Couldn't save the income");
                }
            }
            catch (OperationCanceledException)
            {
                result = SaveResult.Canceled(_income);
                _logger.LogInformation("Operation is canceled by user");
            }
            catch(Exception e)
            {
                result = SaveResult.Failed(_income, e.Message);
                _logger.LogInformation($"Couldn't save the income: {e}");
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