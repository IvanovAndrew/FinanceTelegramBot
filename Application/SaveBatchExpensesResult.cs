using Domain;

namespace Application;

public class SaveBatchExpensesResult
{
    private readonly eSaveExpense _type;
    private readonly IReadOnlyCollection<IMoneyTransfer> _expenses;
    private readonly string _message;

    private SaveBatchExpensesResult(eSaveExpense type, IReadOnlyCollection<IMoneyTransfer> expenses, string message)
    {
        _type = type;
        _expenses = expenses;
        _message = message;
    }

    public static SaveBatchExpensesResult Saved(IReadOnlyCollection<IMoneyTransfer> expenses) => new(eSaveExpense.Saved, expenses, 
        $"All expenses are saved with the following options: {Environment.NewLine}" +
        string.Join($"{Environment.NewLine}",
            $"Date: {expenses.First().Date:dd.MM.yyyy}",
            $"Category: {expenses.First().Category.Name}",
            $"Total Amount: {expenses.Aggregate(new Money(){Amount = 0, Currency = expenses.First().Amount.Currency}, (money, expense) => money + expense.Amount)}"));
    
    public static SaveBatchExpensesResult Canceled(IReadOnlyCollection<IMoneyTransfer> expenses) => new(eSaveExpense.Canceled, expenses, "Canceled by a user");
    
    public static SaveBatchExpensesResult Failed(IReadOnlyCollection<IMoneyTransfer> expenses, string message) => new(eSaveExpense.Error, expenses, $"Failed to save: {message}");

    public string GetMessage()
    {
        return _message;
    }
}