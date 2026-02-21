using Application;

namespace Infrastructure;

public class ExpenseHistoryCategorizer : IExpenseCategorizer
{
    public ExpenseCategorizerResult? GetCategory(string title, IReadOnlyDictionary<string, ExpenseCategorizerResult> availableOptions)
    {
        return availableOptions.GetValueOrDefault(title);
    }
}
