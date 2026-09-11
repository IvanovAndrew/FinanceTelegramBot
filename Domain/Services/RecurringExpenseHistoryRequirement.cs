namespace Domain.Services;

public static class RecurringExpenseHistoryRequirement
{
    public static DateOnly EarliestRequiredDate(
        IReadOnlyCollection<RecurringExpenseDefinition> definitions, DateOnly today)
    {
        var previousPeriodStarts = definitions
            .Where(d => d.Way == Way.PriorPeriod)
            .Select(d => RecurringPeriodCalculator.PreviousPeriod(d.Frequency, today).From)
            .ToList();

        return previousPeriodStarts.Count == 0 ? today : previousPeriodStarts.Min();
    }
}