using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

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
        throw new InvalidOperationException();
    }

    public async Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        // TODO move calculation logic to here
        await Task.Run(() => { });
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
    {
        return _factory.CreateGreetingState();
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
                                 saved? "Saved" : "Saving is cancelled"
                             );
            
        _logger.LogInformation(infoMessage);

        await botClient.DeleteMessageAsync(message.ChatId, savingMessage.Id, cancellationToken);
        return await botClient.SendTextMessageAsync(message.ChatId, infoMessage);
    }

    public void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}