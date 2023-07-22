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

        using (_cancellationTokenSource = new CancellationTokenSource())
        {
            await _expenseRepository.SaveAll(_expenses, cancellationToken);
        }

        _cancellationTokenSource = null;

        _logger.LogInformation($"{_expenses.Count} expenses saved");

        await botClient.DeleteMessageAsync(message.ChatId, message.Id, cancellationToken);
        await botClient.DeleteMessageAsync(message.ChatId, savingMessage.Id, cancellationToken);
        return await botClient.SendTextMessageAsync(message.ChatId, $"{_expenses.Count} expenses saved");
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
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