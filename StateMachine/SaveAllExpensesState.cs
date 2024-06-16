using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

public class SaveAllExpensesState : IExpenseInfoState, ILongTermOperation
{
    private readonly List<IExpense> _expenses;
    private readonly IExpenseRepository _expenseRepository;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    public IExpenseInfoState PreviousState { get; private set; }

    internal SaveAllExpensesState(IExpenseInfoState previousState, List<IExpense> expenses,
        IExpenseRepository expenseRepository, ILogger logger)
    {
        _expenses = expenses;
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
                await _expenseRepository.SaveAll(_expenses, _cancellationTokenSource.Token);
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

        await botClient.DeleteMessageAsync(message, cancellationToken);
        await botClient.DeleteMessageAsync(savingMessage, cancellationToken);

        if (!saved)
        {
            return await botClient.SendTextMessageAsync(message.ChatId, $"Saving is canceled");
        }
        
        _logger.LogInformation($"{_expenses.Count} expenses saved");
        return await botClient.SendTextMessageAsync(message.ChatId, $"{_expenses.Count} expenses saved");
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException();
    }

    public void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}