namespace Domain;

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