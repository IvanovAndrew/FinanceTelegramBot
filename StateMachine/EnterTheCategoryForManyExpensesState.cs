using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class EnterTheCategoryForManyExpensesState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly List<IExpense> _expenses;
    private List<IExpense> _expensesFromJson;
    public bool UserAnswerIsRequired => true;
    public IExpenseInfoState PreviousState { get; }
    private readonly ILogger _logger;

    internal EnterTheCategoryForManyExpensesState(StateFactory factory, IExpenseInfoState previousState, List<IExpense> expenses, ILogger logger)
    {
        _factory = factory;
        _expenses = expenses;
        _logger = logger;
        PreviousState = previousState;
        _expensesFromJson = new List<IExpense>();
    }
        
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Paste json",
            cancellationToken: cancellationToken);
    }

    public async Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        await Task.Run(() => _expensesFromJson.AddRange(new ExpenseJsonParser().Parse(message.Text, "Еда", Currency.Rur)));
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
    {
        // ask about category and subcategory of all expenses
        if (_expensesFromJson.Count == 0)
        {
            _logger.LogInformation("json doesn't contain any info about expenses");
            return _factory.CreateGreetingState();
        }
        else
        {
            return _factory.CreateEnterTheCategoryForManyExpenses(_expensesFromJson, this);
        }
    }
}