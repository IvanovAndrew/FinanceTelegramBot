namespace Application.Core;

public static class DateOnlyExtension
{
    public static DateOnly LastDayOfMonth(this DateOnly date)
    {
        return date.AddMonths(1).FirstDayOfMonth().AddDays(-1);
    }

    public static DateOnly FirstDayOfMonth(this DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, 1);
    }
}

public static class DateOnlyUtils
{
    public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}
