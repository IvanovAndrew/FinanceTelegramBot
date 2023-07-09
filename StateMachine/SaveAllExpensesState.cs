using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

public class SaveAllExpensesState : IExpenseInfoState, ILongTermOperation
{
    private readonly StateFactory _factory;
    private readonly List<IExpense> _expenses;
    private readonly IExpenseRepository _expenseRepository;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    public IExpenseInfoState PreviousState { get; private set; }

    internal SaveAllExpensesState(StateFactory factory, IExpenseInfoState previousState, List<IExpense> expenses,
        IExpenseRepository expenseRepository, ILogger logger)
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

        _logger.LogInformation($"{_expenses.Count} expenses saved");

        await botClient.DeleteMessageAsync(chatId, message.Id, cancellationToken);
        return await botClient.SendTextMessageAsync(chatId, $"{_expenses.Count} expenses saved");
    }

    public IExpenseInfoState Handle(IMessage message, CancellationToken cancellationToken)
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