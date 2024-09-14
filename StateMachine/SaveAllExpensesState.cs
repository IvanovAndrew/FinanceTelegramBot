using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

public class SaveAllExpensesState : IExpenseInfoState, ILongTermOperation
{
    private readonly List<IExpense> _expenses;
    private readonly IFinanseRepository _finanseRepository;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    internal SaveAllExpensesState(List<IExpense> expenses,
        IFinanseRepository finanseRepository, ILogger logger)
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
        if (TelegramCommand.TryGetCommand(message.Text, out _))
        {
            Cancel();
        }
        
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        Cancel();
        throw new NotImplementedException();
    }

    public async Task<IMessage> Handle(ITelegramBot botClient, IMessage message, CancellationToken cancellationToken)
    {
        var savingMessage = await botClient.SendTextMessageAsync(message.ChatId, "Saving... It can take some time.");

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
        catch (Exception exception)
        {
            result = SaveBatchExpensesResult.Failed(_expenses, exception.Message);
        }
        finally
        {
            _cancellationTokenSource = null;
        }

        await botClient.DeleteMessageAsync(message, cancellationToken);
        await botClient.DeleteMessageAsync(savingMessage, cancellationToken);

        return await botClient.SendTextMessageAsync(message.ChatId, result.GetMessage());
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException();
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