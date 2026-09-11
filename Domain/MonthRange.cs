namespace Domain;

public record MonthRange
{
    public YearMonth From { get; init; }
    public YearMonth To { get; init; } = new YearMonth(2099, 12);

    public bool IsInRange(DateOnly date)
    {
        var yearMonth = YearMonth.From(date);
        return From <= yearMonth && yearMonth <= To;
    }
}