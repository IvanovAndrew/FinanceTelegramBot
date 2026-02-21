namespace Domain;

public sealed class SpendingHistoryPeriod
{
    public DateOnly From { get; }
    public DateOnly To { get; }

    private SpendingHistoryPeriod(DateOnly from, DateOnly to)
    {
        From = from;
        To = to;
    }

    public static SpendingHistoryPeriod FromCalculationStart(
        DateOnly today,
        YearMonth statisticsFrom)
    {
        var calculationMonth = YearMonth.From(today);

        if (calculationMonth <= statisticsFrom)
        {
            var previousMonth = statisticsFrom.Previous();

            return new SpendingHistoryPeriod(previousMonth.ToDateOnly(1), today);
        }

        return new SpendingHistoryPeriod(statisticsFrom.ToDateOnly(1), today);
    }
}
