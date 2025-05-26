namespace Domain;

public class ExpenseFromCategoryAndSubcategorySpecification(Category category, SubCategory subCategory)
    : ExpenseFromCategorySpecification(category)
{
    public override bool IsSatisfied(IMoneyTransfer item)
    {
        return base.IsSatisfied(item) && item.SubCategory == subCategory;
    }
}