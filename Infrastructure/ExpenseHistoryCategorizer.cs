namespace Infrastructure;

public class ExpenseHistoryCategorizer : IExpenseCategorizer
{
    public ExpenseCategorizerResult? GetCategory(string title, Dictionary<string, ExpenseCategorizerResult> availableOptions)
    {
        return availableOptions.GetValueOrDefault(title);
    }
}
