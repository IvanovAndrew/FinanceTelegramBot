using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class EnterTheCategoryForManyExpensesState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly List<IExpense> _expenses;
    public bool UserAnswerIsRequired => true;
    public IExpenseInfoState PreviousState { get; }
    private readonly ILogger _logger;
        
    internal EnterTheCategoryForManyExpensesState(StateFactory factory, IExpenseInfoState previousState, List<IExpense> expenses, ILogger logger)
    {
        _factory = factory;
        _expenses = expenses;
        _logger = logger;
        PreviousState = previousState;
    }
        
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Paste json",
            cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(IMessage message, CancellationToken cancellationToken)
    {
        var expensesFromJson = new ExpenseJsonParser().Parse(message.Text, "Еда", Currency.Rur);
        
        // ask about category and subcategory of all expenses
        if (expensesFromJson.Count == 0)
        {
            _logger.LogInformation("json doesn't contain any info about expenses");
            return _factory.CreateGreetingState();
        }
        else
        {
            return _factory.CreateEnterTheCategoryForManyExpenses(expensesFromJson, this);
        }

    }
}