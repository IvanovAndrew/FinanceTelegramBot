namespace Domain;

public class ExpenseFromSubcategorySpecification : ISpecification<IMoneyTransfer>
{
    private readonly Category _category;
    private readonly SubCategory _subCategory;

    public ExpenseFromSubcategorySpecification(Category category, SubCategory subCategory)
    {
        _category = category;
        _subCategory = subCategory;
    }
    
    public bool IsSatisfied(IMoneyTransfer item)
    {
        return string.Equals(item.Category, _category.Name, StringComparison.InvariantCultureIgnoreCase) &&
               string.Equals(item.SubCategory, _subCategory.Name, StringComparison.InvariantCultureIgnoreCase);
    }
}