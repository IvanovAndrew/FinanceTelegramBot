namespace Domain;

public static class BudgetPlanner
{
    public static Money Plan(Money balance, int remainingDays, IReadOnlyList<Money> actualExpenses, IReadOnlyList<Money> compulsoryLeftToPay)
    {
        if (remainingDays < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(remainingDays), "Number of days must be positive.");
        }
        
        var zero = Money.Zero(balance.Currency);
        
        var expenses = actualExpenses.Aggregate(zero, (current, outcome) => current + outcome);

        var allMandatoryOutcomes = compulsoryLeftToPay.Aggregate(zero, (current, outcome) => current + outcome);

        var leftMoney = balance - expenses - allMandatoryOutcomes;

        if (leftMoney.Amount < 0)
        {
            return zero;
        }

        if (remainingDays == 0)
        {
            return leftMoney;
        }

        return leftMoney / remainingDays;
    }
}