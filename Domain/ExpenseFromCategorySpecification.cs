namespace Domain;

public class ExpenseFromCategorySpecification : ISpecification<IExpense>
{
    private readonly string _category; 
    public ExpenseFromCategorySpecification(Category category) : this(category.Name)
    {
    }
    
    public ExpenseFromCategorySpecification(string category)
    {
        _category = category;
    }
    
    public bool IsSatisfied(IExpense item)
    {
        return string.Equals(item.Category, _category, StringComparison.InvariantCultureIgnoreCase);
    }
}