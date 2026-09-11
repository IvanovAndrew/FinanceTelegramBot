using Domain;

namespace Application.Core;

public readonly record struct ChartBucket
{
    public DateOnly Date { get; }
    public ChartGranularity Granularity { get; }

    private ChartBucket(DateOnly date, ChartGranularity granularity)
    {
        Date = date;
        Granularity = granularity;
    }

    public static ChartBucket ForDay(DateOnly date) => new(date, ChartGranularity.Day);
    public static ChartBucket ForMonth(YearMonth month) => new(month.ToDateOnly(), ChartGranularity.Month);

    public string Label => Granularity switch
    {
        ChartGranularity.Day => Date.ToString(DateFormat.DayWithoutYear),
        ChartGranularity.Month => Date.ToString(DateFormat.FullMonthName),
        _ => Date.ToString()
    };
}

public enum ChartGranularity { Day, Month }