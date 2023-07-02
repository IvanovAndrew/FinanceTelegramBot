namespace Domain;

public class ExpenseLaterThanSpecification : ISpecification<IExpense>
{
    private readonly DateOnly _startDate;

    public ExpenseLaterThanSpecification(DateOnly startDate)
    {
        _startDate = startDate;
    }
    public bool IsSatisfied(IExpense item)
    {
        return _startDate <= item.Date;
    }
}