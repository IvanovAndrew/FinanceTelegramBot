namespace Infrastructure;

static class DateOnlyExtension
{
    internal static DateOnly LastDayOfMonth(this DateOnly date)
    {
        var lastDayOfMonth = new [] {0, 31, date.Year % 4 == 0 ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};

        return new DateOnly(date.Year, date.Month, lastDayOfMonth[date.Month]);
    }
        
    internal static DateOnly FirstDayOfMonth(this DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, 1);
    }
}