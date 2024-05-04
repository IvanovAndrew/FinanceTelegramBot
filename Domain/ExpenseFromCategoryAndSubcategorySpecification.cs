namespace Domain;

public class ExpenseFromCategoryAndSubcategorySpecification : ExpenseFromCategorySpecification
{
    private readonly SubCategory _subCategory;
    public ExpenseFromCategoryAndSubcategorySpecification(Category category, SubCategory subCategory) : base(category)
    {
        _subCategory = subCategory;
    }
    
    public override bool IsSatisfied(IExpense item)
    {
        return base.IsSatisfied(item) && string.Equals(item.SubCategory, _subCategory.Name, StringComparison.InvariantCultureIgnoreCase);
    }
}