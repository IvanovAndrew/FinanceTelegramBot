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
        DateOnly statisticsFrom)
    {
        var calculationMonth = YearMonth.From(today);
        var statisticsMonth = YearMonth.From(statisticsFrom);

        if (calculationMonth <= statisticsMonth)
        {
            var previousMonth = statisticsMonth.Previous();

            return new SpendingHistoryPeriod(previousMonth.ToDateOnly(1), today);
        }

        return new SpendingHistoryPeriod(statisticsMonth.ToDateOnly(1), today);
    }
}
