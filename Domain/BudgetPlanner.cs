namespace Domain;

public static class BudgetPlanner
{
    public static Money Plan(Money balance, int days, IReadOnlyList<Money> compulsoryOutcomes)
    {
        if (days <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(days), "Number of days must be positive.");
        }
        
        var zero = Money.Zero(balance.Currency);

        var allMandatoryOutcomes = compulsoryOutcomes.Aggregate(zero, (current, outcome) => current + outcome);

        var leftMoney = balance - allMandatoryOutcomes;

        if (leftMoney.Amount < 0)
        {
            return zero;
        }

        return leftMoney / days;
    }
}