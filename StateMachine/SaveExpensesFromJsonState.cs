using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class SaveExpensesFromJsonState : IExpenseInfoState, ILongTermOperation
{
    private readonly List<IExpense> _expenses;
    private readonly IFinanseRepository _finanseRepository;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    
    internal SaveExpensesFromJsonState(List<IExpense> expenses, IFinanseRepository finanseRepository, ILogger logger)
    {
        _expenses = expenses;
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
        // TODO move calculation logic to here
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        throw new NotImplementedException();
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        return stateFactory.CreateGreetingState();
    }

    public async Task<IMessage> Handle(ITelegramBot botClient, IMessage message, CancellationToken cancellationToken)
    {
        if (TelegramCommand.TryGetCommand(message.Text, out _))
        {
            Cancel();
        }
        else
        {
            var savingMessage =
                await botClient.SendTextMessageAsync(message.ChatId, "Saving... It can take some time.");

            SaveBatchExpensesResult result;
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    await _finanseRepository.SaveAllOutcomes(_expenses, _cancellationTokenSource.Token);
                }

                result = SaveBatchExpensesResult.Saved(_expenses);
            }
            catch (OperationCanceledException)
            {
                result = SaveBatchExpensesResult.Canceled(_expenses);
                _logger.LogInformation("Operation is canceled by user");
            }
            catch (Exception e)
            {
                result = SaveBatchExpensesResult.Failed(_expenses, e.Message);
            }
            finally
            {
                _cancellationTokenSource = null;
            }

            var sum = new Money() { Amount = 0, Currency = _expenses[0].Amount.Currency };
            foreach (var expense in _expenses)
            {
                sum += expense.Amount;
            }

            await botClient.DeleteMessageAsync(savingMessage, cancellationToken);
            return await botClient.SendTextMessageAsync(message.ChatId, result.GetMessage());
        }

        return await botClient.SendTextMessageAsync(message.ChatId, "Canceled");
    }

    public Task Cancel()
    {
        return Task.Run(() =>
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        });
    }
}