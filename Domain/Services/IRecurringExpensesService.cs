namespace Domain.Services;

public record MissingRecurringExpense(RecurringExpenseDefinition Definition, Money? ResolvedAmount);

public interface IRecurringExpensesService
{
    IReadOnlyList<MissingRecurringExpense> GetMissingRecurringExpenses(
        IReadOnlyCollection<RecurringExpenseDefinition> definitions,
        IEnumerable<Outcome> allExpenses,
        DateOnly today);
}

public class RecurringExpensesService : IRecurringExpensesService
{
    public IReadOnlyList<MissingRecurringExpense> GetMissingRecurringExpenses(
        IReadOnlyCollection<RecurringExpenseDefinition> definitions,
        IEnumerable<Outcome> allExpenses,
        DateOnly today)
    {
        var history = allExpenses as IReadOnlyCollection<Outcome> ?? allExpenses.ToList();
        var missing = new List<MissingRecurringExpense>();

        foreach (var def in definitions)
        {
            foreach (var period in RecurringPeriodCalculator.PeriodsUntilEndOfMonth(def.Frequency, today))
            {
                var alreadyPaid = history.Any(e =>
                    e.Category == def.Category && e.SubCategory == def.SubCategory && e.Shop == def.Shop &&
                    period.Contains(e.Date));

                if (alreadyPaid) continue;

                missing.Add(new MissingRecurringExpense(def, Resolve(def, history, today)));
            }
        }

        return missing;
    }

    private static Money? Resolve(RecurringExpenseDefinition def, IReadOnlyCollection<Outcome> history, DateOnly today) =>
        def.Way switch
        {
            Way.Fixed => def.ExpectedAmount,
            Way.PriorPeriod => ResolveFromPriorPeriod(def, history, today),
            _ => throw new ArgumentOutOfRangeException(nameof(def.Way))
        };

    private static Money? ResolveFromPriorPeriod(RecurringExpenseDefinition def, IReadOnlyCollection<Outcome> history, DateOnly today)
    {
        var previousPeriod = RecurringPeriodCalculator.PreviousPeriod(def.Frequency, today);

        var match = history
            .Where(e => e.Category == def.Category && e.SubCategory == def.SubCategory && e.Shop == def.Shop)
            .Where(e => previousPeriod.Contains(e.Date))
            .ToList();

        if (match.Count == 0) return null; // no expense for the previous period, cannot resolve

        var currency = match[0].Amount.Currency;
        return match.Aggregate(Money.Zero(currency), (sum, e) => sum + e.Amount);
    }
}