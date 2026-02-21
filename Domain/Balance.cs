using Domain.Services;

namespace Domain;

public readonly record struct MonthlyBalance(YearMonth Month, Balance Balance);


public readonly struct Balance(Money income, Money outcome)
{
    public readonly Money Income = income;
    public readonly Money Outcome = outcome;
    public Money Saldo => Income - Outcome;
    
    public static Balance operator +(Balance first, Balance second)
    {
        return new Balance(first.Income + second.Income, first.Outcome + second.Outcome);
    }

    public override string ToString()
    {
        return $"Income = {Income} Outcome = {Outcome}";
    }
}

public readonly struct YearMonth : IEquatable<YearMonth>, IComparable<YearMonth>
{
    public readonly int Year;
    public readonly int Month;
    
    public YearMonth(int year, int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month));
        
        if (year < 2000 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year));
        
        Year = year;
        Month = month;
    }
    
    public static YearMonth From(DateOnly date) => new(date.Year, date.Month);
    public static YearMonth From(DateTime date) => new(date.Year, date.Month);

    public YearMonth Next()
    {
        return Month == 12
            ? new YearMonth(Year + 1, 1)
            : new YearMonth(Year, Month + 1);
    }
    
    public YearMonth Previous()
    {
        return Month == 1? new YearMonth(Year - 1, 12): new YearMonth(Year, Month - 1);
    }

    public DateOnly ToDateOnly(int day = 1) => new DateOnly(Year, Month, 1);
    public DateOnly ToLastDayOfMonth() => new DateOnly(Year, Month + 1, 1).AddDays(-1);
    public DateTime ToDateTime(int day = 1) => new DateTime(Year, Month, 1);

    public bool Equals(YearMonth other)
    {
        return Year == other.Year && Month == other.Month;
    }

    public override bool Equals(object? obj)
    {
        return obj is YearMonth other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Year, Month);
    }

    public int CompareTo(YearMonth other)
    {
        var yearComparison = Year.CompareTo(other.Year);
        if (yearComparison != 0) return yearComparison;
        return Month.CompareTo(other.Month);
    }
    
    public static bool operator <=(YearMonth first, YearMonth second)
    {
        return first.CompareTo(second) <= 0;
    }
    
    public static bool operator <(YearMonth first, YearMonth second)
    {
        return first.CompareTo(second) < 0;
    }
    
    public static bool operator >=(YearMonth first, YearMonth second)
    {
        return first.CompareTo(second) >= 0;
    }
    
    public static bool operator >(YearMonth first, YearMonth second)
    {
        return first.CompareTo(second) > 0;
    }
    
    public static bool operator ==(YearMonth first, YearMonth second)
    {
        return first.Equals(second);
    }

    public static bool operator !=(YearMonth first, YearMonth second)
    {
        return !(first == second);
    }

    public override string ToString() => $"{Year:D4}-{Month:D2}";
    
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        if (string.IsNullOrEmpty(format))
            format = "yyyy-MM";

        var provider = formatProvider ?? System.Globalization.CultureInfo.CurrentCulture;
        
        // Apply years
        var result = format;

        var before = result;
        result = before.Replace("yyyy", Year.ToString("D4", provider));
        if (result == before)
        {
            result = result.Replace("yy", (Year % 100).ToString("D2", provider));
        }

        before = result;

        result = result.Replace("MMMM", new System.Globalization.DateTimeFormatInfo().GetMonthName(Month));

        if (before != result)
            return result;

        result = result.Replace("MMM", new System.Globalization.DateTimeFormatInfo().GetAbbreviatedMonthName(Month));
        if (before != result)
            return result;


        result = result.Replace("MM", Month.ToString("D2", provider));
        if (before != result)
            return result;
        
        return result.Replace("M", Month.ToString(provider));
    }
}

public class BalancePeriod(
    IEnumerable<IMoneyTransfer> incomes,
    IEnumerable<IMoneyTransfer> outcomes,
    Currency currency)
{
    private readonly IReadOnlyList<IMoneyTransfer> _incomes = incomes.ToList();
    private readonly IReadOnlyList<IMoneyTransfer> _outcomes = outcomes.ToList();

    public IReadOnlyList<MonthlyBalance> ByMonths(MonthRange monthRange)
    {
        var result = new List<MonthlyBalance>();
        var month = monthRange.From;

        while (month <= monthRange.To)
        {
            result.Add(new MonthlyBalance(
                month,
                new Balance(
                    Sum(_incomes, month),
                    Sum(_outcomes, month)
                )
            ));

            month = month.Next();
        }

        return result;
    }

    private Money Sum(
        IEnumerable<IMoneyTransfer> items,
        YearMonth yearMonth) => Sum(items, new MonthRange {From =  yearMonth, To = yearMonth});

    private Money Sum(
        IEnumerable<IMoneyTransfer> items,
        MonthRange range)
    {
        return FinanceCalculator.Sum(items, currency, range);
    }

    public Money TotalIncome => _incomes.Where(c => c.Amount.Currency == currency).Select(_ => _.Amount).Aggregate(Money.Zero(currency), (acc, money) => acc + money);

    public Money TotalOutcome =>
        _outcomes.Where(c => c.Amount.Currency == currency).Select(_ => _.Amount).Aggregate(Money.Zero(currency), (acc, money) => acc + money);

    public Money Saldo => TotalIncome - TotalOutcome;
}

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