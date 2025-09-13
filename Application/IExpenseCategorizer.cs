using Domain;

public interface IExpenseCategorizer
{
    public ExpenseCategorizerResult? GetCategory(string title,
        Dictionary<string, ExpenseCategorizerResult> availableOptions);
}

public record ExpenseCategorizerResult
{
    public Category Category { get; init; }
    public SubCategory? SubCategory { get; init; }

    public static ExpenseCategorizerResult Create(Category category, SubCategory? subCategory = null)
    {
        return new ExpenseCategorizerResult() { Category = category, SubCategory = subCategory };
    }
}