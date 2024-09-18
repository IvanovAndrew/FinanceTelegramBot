namespace Domain;

public class ExpenseFromCategorySpecification : ISpecification<IMoneyTransfer>
{
    private readonly string _category;
    public ExpenseFromCategorySpecification(Category category) : this(category.Name)
    {
    }
    
    public ExpenseFromCategorySpecification(string category)
    {
        _category = category;
    }
    
    public virtual bool IsSatisfied(IMoneyTransfer item)
    {
        return string.Equals(item.Category, _category, StringComparison.InvariantCultureIgnoreCase);
    }
}