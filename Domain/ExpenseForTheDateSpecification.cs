namespace Domain;

public class ExpenseForTheDateSpecification : ExpenseFromDateRangeSpecification
{
    public ExpenseForTheDateSpecification(DateOnly date) : base(date, date)
    {
    }
}