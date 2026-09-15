namespace Domain.Services;

public readonly record struct PeriodRange(DateOnly From, DateOnly To)
{
    public bool Contains(DateOnly date) => date >= From && date <= To;
}

public static class RecurringPeriodCalculator
{
    public static IEnumerable<PeriodRange> PeriodsUntilEndOfMonth(RecurringFrequency frequency, DateOnly today)
    {
        if (frequency == RecurringFrequency.Monthly)
        {
            yield return CurrentPeriod(frequency, today);
            yield break;
        }

        var monthEnd = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        var step = frequency == RecurringFrequency.Weekly ? 7 : 14;
        var period = CurrentPeriod(frequency, today);

        while (period.From <= monthEnd)
        {
            yield return period;
            period = new PeriodRange(period.From.AddDays(step), period.To.AddDays(step));
        }
    }
    
    public static PeriodRange CurrentPeriod(RecurringFrequency frequency, DateOnly today) => frequency switch
    {
        RecurringFrequency.Monthly => MonthRange(YearMonth.From(today)),
        RecurringFrequency.Weekly => new PeriodRange(today.AddDays(-6), today),
        RecurringFrequency.Biweekly => new PeriodRange(today.AddDays(-13), today),
        _ => throw new ArgumentOutOfRangeException(nameof(frequency))
    };

    public static PeriodRange PreviousPeriod(RecurringFrequency frequency, DateOnly today) => frequency switch
    {
        RecurringFrequency.Monthly => MonthRange(YearMonth.From(today).Previous()),
        RecurringFrequency.Weekly => new PeriodRange(today.AddDays(-13), today.AddDays(-7)),
        RecurringFrequency.Biweekly => new PeriodRange(today.AddDays(-27), today.AddDays(-14)),
        _ => throw new ArgumentOutOfRangeException(nameof(frequency))
    };

    private static PeriodRange MonthRange(YearMonth month)
    {
        var lastDay = DateTime.DaysInMonth(month.Year, month.Month);
        return new PeriodRange(new DateOnly(month.Year, month.Month, 1), new DateOnly(month.Year, month.Month, lastDay));
    }
}