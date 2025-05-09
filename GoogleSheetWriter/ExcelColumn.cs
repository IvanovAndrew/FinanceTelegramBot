using static GoogleSheetWriter.AlphabetConstants;

namespace GoogleSheetWriter;

public class AlphabetConstants
{
    public const char FirstLetter = 'A';
    public const int LettersInLatinAlphabet = 26;
}

public class ExcelColumn : IEquatable<ExcelColumn>, IComparable<ExcelColumn>
{
    private readonly string _name;
    public string Name => _name;

    private ExcelColumn(string name)
    {
        _name = name;
    }

    public static ExcelColumn FromString(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("Excel column name must not be null or whitespace.", nameof(s));

        var upper = s.ToUpperInvariant();
        foreach (var ch in upper)
        {
            if (!char.IsLetter(ch))
                throw new ArgumentException($"Incorrect excel column name {s}: {ch}");
        }

        return new ExcelColumn(upper);
    }

    public int ToNumber()
    {
        int number = 0;

        foreach (char c in _name)
        {
            number = number * LettersInLatinAlphabet + (c - FirstLetter + 1);
        }

        return number;
    }
    
    public static ExcelColumn FromNumber(int number)
    {
        if (number < 1)
            throw new ArgumentOutOfRangeException(nameof(number));

        var cell = "";
        while (number > 0)
        {
            number--;
            cell = (char)(FirstLetter + number % LettersInLatinAlphabet) + cell;
            number /= LettersInLatinAlphabet;
        }
        return new ExcelColumn(cell);
    }

    public static int DifferenceBetween(ExcelColumn one, ExcelColumn two)
    {
        return one.ToNumber() - two.ToNumber();
    }

    public bool Equals(ExcelColumn? other)
    {
        if (ReferenceEquals(other, null))
            return false;

        return string.Equals(_name, other._name, StringComparison.InvariantCultureIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ExcelColumn)obj);
    }

    public override int GetHashCode()
    {
        return StringComparer.InvariantCultureIgnoreCase.GetHashCode(_name);
    }
    
    public int CompareTo(ExcelColumn? other)
    {
        if (other == null) return 1;
        return ToNumber().CompareTo(other.ToNumber());
    }
    
    public static bool operator <(ExcelColumn left, ExcelColumn right) => left.CompareTo(right) < 0;
    public static bool operator >(ExcelColumn left, ExcelColumn right) => left.CompareTo(right) > 0;
    public static bool operator <=(ExcelColumn left, ExcelColumn right) => left.CompareTo(right) <= 0;
    public static bool operator >=(ExcelColumn left, ExcelColumn right) => left.CompareTo(right) >= 0;

    public override string ToString()
    {
        return Name;
    }

    public static ExcelColumn[] ColumnsBetween(ExcelColumn firstColumn, ExcelColumn lastColumn)
    {
        if (firstColumn is null)
            throw new ArgumentNullException(nameof(firstColumn));
        if (lastColumn is null)
            throw new ArgumentNullException(nameof(lastColumn));

        int start = firstColumn.ToNumber();
        int end = lastColumn.ToNumber();

        if (start > end)
            (start, end) = (end, start);

        var result = new List<ExcelColumn>();
        for (int i = start; i <= end; i++)
        {
            result.Add(FromNumber(i));
        }

        return result.ToArray();
    }
}