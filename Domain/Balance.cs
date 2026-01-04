using Domain.Services;

namespace Domain;

public readonly record struct MonthlyBalance(YearMonth Month, Balance Balance);


public readonly struct Balance(Money income, Money outcome)
{
    public readonly Money Income = income;
    public readonly Money Outcome = outcome;
    public readonly Money Saldo => Income - Outcome;
    
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

}

public class BalancePeriod(
    IEnumerable<IMoneyTransfer> incomes,
    IEnumerable<IMoneyTransfer> outcomes,
    Currency currency)
{
    private readonly IReadOnlyList<IMoneyTransfer> _incomes = incomes.ToList();
    private readonly IReadOnlyList<IMoneyTransfer> _outcomes = outcomes.ToList();

    public IReadOnlyList<MonthlyBalance> ByMonths(
        YearMonth from,
        YearMonth to)
    {
        var result = new List<MonthlyBalance>();
        var month = from;

        while (month <= to)
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
        YearMonth month)
    {
        var start = new DateOnly(month.Year, month.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return FinanceCalculator.Sum(items, currency, start, end);
    }

    public Money TotalIncome => _incomes.Select(_ => _.Amount).Aggregate(Money.Zero(currency), (acc, money) => acc + money);

    public Money TotalOutcome =>
        _outcomes.Select(_ => _.Amount).Aggregate(Money.Zero(currency), (acc, money) => acc + money);
}
