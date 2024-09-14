using Domain;

public class SaveResult
{
    private readonly eSaveExpense _type;
    private readonly IExpense? _expense;
    private readonly IIncome? _income;
    private readonly string _message;

    private SaveResult(eSaveExpense type, IExpense expense, string message)
    {
        _type = type;
        _expense = expense;
        _message = message;
    }
    
    private SaveResult(eSaveExpense type, IIncome income, string message)
    {
        _type = type;
        _income = income;
        _message = message;
    }

    public static SaveResult Saved(IExpense expense) => new(eSaveExpense.Saved, expense, "Saved");
    public static SaveResult Saved(IIncome income) => new(eSaveExpense.Saved, income, "Saved");
    
    public static SaveResult Canceled(IExpense expense) => new(eSaveExpense.Canceled, expense, "Canceled by a user");
    public static SaveResult Canceled(IIncome income) => new(eSaveExpense.Canceled, income, "Canceled by a user");
    
    public static SaveResult Failed(IExpense expense, string message) => new(eSaveExpense.Error, expense, $"Failed to save: {message}");
    public static SaveResult Failed(IIncome income, string message) => new(eSaveExpense.Error, income, $"Failed to save: {message}");

    public string GetMessage()
    {
        if (_expense != null)
        {
            return string.Join($"{Environment.NewLine}",
                $"Date: {_expense.Date:dd.MM.yyyy}",
                $"Category: {_expense.Category}",
                $"Subcategory: {_expense.SubCategory ?? string.Empty}",
                $"Description: {_expense.Description ?? string.Empty}",
                $"Amount: {_expense.Amount}",
                "",
                _message
            );
        }
        else if (_income != null)
        {
            return string.Join($"{Environment.NewLine}",
                $"Date: {_income.Date:dd.MM.yyyy}",
                $"Category: {_income.Category}",
                $"Description: {_income.Description ?? string.Empty}",
                $"Amount: {_income.Amount}",
                "",
                _message
            );
        }

        throw new InvalidOperationException("Neither outcome nor income are presented");
    }
}

public class SaveBatchExpensesResult
{
    private readonly eSaveExpense _type;
    private readonly List<IExpense> _expenses;
    private readonly string _message;

    private SaveBatchExpensesResult(eSaveExpense type, List<IExpense> expenses, string message)
    {
        _type = type;
        _expenses = expenses;
        _message = message;
    }

    public static SaveBatchExpensesResult Saved(List<IExpense> expenses) => new(eSaveExpense.Saved, expenses, 
        $"All expenses are saved with the following options: {Environment.NewLine}" +
        string.Join($"{Environment.NewLine}",
            $"Date: {expenses[0].Date:dd.MM.yyyy}",
            $"Category: {expenses[0].Category}",
            $"Total Amount: {expenses.Aggregate(new Money(){Amount = 0, Currency = expenses[0].Amount.Currency}, (money, expense) => money + expense.Amount)}"));
    
    public static SaveBatchExpensesResult Canceled(List<IExpense> expenses) => new(eSaveExpense.Canceled, expenses, "Canceled by a user");
    
    public static SaveBatchExpensesResult Failed(List<IExpense> expenses, string message) => new(eSaveExpense.Error, expenses, $"Failed to save: {message}");

    public string GetMessage()
    {
        return _message;
    }
}