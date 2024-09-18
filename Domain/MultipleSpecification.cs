namespace Domain;

public class MultipleSpecification : ISpecification<IMoneyTransfer>
{
    private readonly ISpecification<IMoneyTransfer>[] _conditions;
    public MultipleSpecification(params ISpecification<IMoneyTransfer>[] conditions)
    {
        _conditions = conditions?? throw new ArgumentNullException(nameof(conditions));
    }
    
    public bool IsSatisfied(IMoneyTransfer item)
    {
        foreach (var specification in _conditions)
        {
            if (!specification.IsSatisfied(item))
                return false;
        }

        return true;
    }
}