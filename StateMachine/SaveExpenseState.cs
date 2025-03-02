using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class SaveExpenseState : IExpenseInfoState, ILongTermOperation
    {
        private readonly IMoneyTransfer _moneyTransfer;
        private readonly IFinanceRepository _financeRepository;
        private readonly ILogger _logger;
        private CancellationTokenSource? _cancellationTokenSource;
    
        internal SaveExpenseState(IMoneyTransfer moneyTransfer, IFinanceRepository financeRepository, ILogger logger)
        {
            _moneyTransfer = moneyTransfer;
            _financeRepository = financeRepository;
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
            return stateFactory.CreateConfirmState(_moneyTransfer);
        }

        public async Task<IMessage> Handle(ITelegramBot botClient, IMessage message, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(new EditableMessageToSend(){ChatId = message.ChatId, Text = "Saving... It can take some time."}, cancellationToken: cancellationToken);

            SaveResult result;
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    var saved = _moneyTransfer.IsIncome? await _financeRepository.SaveIncome(_moneyTransfer, _cancellationTokenSource.Token) 
                        : await _financeRepository.SaveOutcome(_moneyTransfer, _cancellationTokenSource.Token);
                    
                    result = saved ? SaveResult.Saved(_moneyTransfer) : SaveResult.Failed(_moneyTransfer, "");
                }
            }
            catch (OperationCanceledException)
            {
                result = SaveResult.Canceled(_moneyTransfer);
                _logger.LogInformation("Operation is canceled by user");
            }
            catch(Exception e)
            {
                result = SaveResult.Failed(_moneyTransfer, e.Message);
                _logger.LogInformation($"Couldn't save an expense: {e}");
            }
            finally
            {
                _cancellationTokenSource = null;
            }

            return await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ ChatId = message.ChatId, Text = result.GetMessage()}, cancellationToken: cancellationToken);
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