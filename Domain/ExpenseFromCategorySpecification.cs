namespace Domain;

public class ExpenseFromCategorySpecification(Category category) : ISpecification<IMoneyTransfer>
{
    public virtual bool IsSatisfied(IMoneyTransfer item)
    {
        return item.Category == category;
    }
}