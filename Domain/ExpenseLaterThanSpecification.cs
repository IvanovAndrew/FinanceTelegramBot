namespace Domain;

public class ExpenseLaterThanSpecification : ISpecification<IMoneyTransfer>
{
    private readonly DateOnly _startDate;

    public ExpenseLaterThanSpecification(DateOnly startDate)
    {
        _startDate = startDate;
    }
    public bool IsSatisfied(IMoneyTransfer item)
    {
        return _startDate <= item.Date;
    }
}