namespace Domain;

public class MultipleSpecification : ISpecification<IExpense>
{
    private readonly ISpecification<IExpense>[] _conditions;
    public MultipleSpecification(params ISpecification<IExpense>[] conditions)
    {
        _conditions = conditions?? throw new ArgumentNullException(nameof(conditions));
    }
    
    public bool IsSatisfied(IExpense item)
    {
        foreach (var specification in _conditions)
        {
            if (!specification.IsSatisfied(item))
                return false;
        }

        return true;
    }
}