using Domain;

namespace StateMachine;

public class SaveResult
{
    private readonly eSaveExpense _type;
    private readonly IMoneyTransfer? _transfer;
    private readonly string _message;

    private SaveResult(eSaveExpense type, IMoneyTransfer moneyTransfer, string message)
    {
        _type = type;
        _transfer = moneyTransfer;
        _message = message;
    }
    
    public static SaveResult Saved(IMoneyTransfer expense) => new(eSaveExpense.Saved, expense, "Saved");
    
    public static SaveResult Canceled(IMoneyTransfer expense) => new(eSaveExpense.Canceled, expense, "Canceled by a user");
    
    public static SaveResult Failed(IMoneyTransfer expense, string message) => new(eSaveExpense.Error, expense, $"Failed to save: {message}");

    public string GetMessage()
    {
        if (_transfer != null)
        {
            return string.Join($"{Environment.NewLine}", 
                _transfer.ToString(),
                "",
                _message);
        }
        
        throw new InvalidOperationException("Neither outcome nor income are presented");
    }
}

public class SaveBatchExpensesResult
{
    private readonly eSaveExpense _type;
    private readonly List<IMoneyTransfer> _expenses;
    private readonly string _message;

    private SaveBatchExpensesResult(eSaveExpense type, List<IMoneyTransfer> expenses, string message)
    {
        _type = type;
        _expenses = expenses;
        _message = message;
    }

    public static SaveBatchExpensesResult Saved(List<IMoneyTransfer> expenses) => new(eSaveExpense.Saved, expenses, 
        $"All expenses are saved with the following options: {Environment.NewLine}" +
        string.Join($"{Environment.NewLine}",
            $"Date: {expenses[0].Date:dd.MM.yyyy}",
            $"Category: {expenses[0].Category}",
            $"Total Amount: {expenses.Aggregate(new Money(){Amount = 0, Currency = expenses[0].Amount.Currency}, (money, expense) => money + expense.Amount)}"));
    
    public static SaveBatchExpensesResult Canceled(List<IMoneyTransfer> expenses) => new(eSaveExpense.Canceled, expenses, "Canceled by a user");
    
    public static SaveBatchExpensesResult Failed(List<IMoneyTransfer> expenses, string message) => new(eSaveExpense.Error, expenses, $"Failed to save: {message}");

    public string GetMessage()
    {
        return _message;
    }
}