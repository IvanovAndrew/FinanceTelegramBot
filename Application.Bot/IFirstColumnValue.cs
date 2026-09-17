using Application.Core;
using Domain;

namespace Application.Bot;

public interface IFirstColumnFactory<T>
{
    public IFirstColumnValue Create(T value);
}

public class StringColumnFactory : IFirstColumnFactory<string>
{
    public IFirstColumnValue Create(string value)
    {
        return new StringColumnValue(value);
    }
}

public class DateOnlyColumnFactory : IFirstColumnFactory<DateOnly>
{
    public IFirstColumnValue Create(DateOnly value)
    {
        return new DateOnlyColumnValue(value);
    }
}

public class MonthColumnFactory : IFirstColumnFactory<YearMonth>
{
    public IFirstColumnValue Create(YearMonth value)
    {
        return new MonthOnlyColumnValue(value);
    }
}

public interface IFirstColumnValue
{
    string GetString();
}

public class StringColumnValue : IFirstColumnValue
{
    public string Value { get; }

    public StringColumnValue(string value) => Value = value;
    public string GetString() => Value;
}

public class DateOnlyColumnValue : IFirstColumnValue
{
    public DateOnly Value { get; }

    public DateOnlyColumnValue(DateOnly value) => Value = value;
    public string GetString() => Value.ToString(DateFormat.FullMonthName);
}

public class MonthOnlyColumnValue : IFirstColumnValue
{
    public YearMonth Value { get; }

    public MonthOnlyColumnValue(YearMonth value) => Value = value;
    public string GetString() => Value.ToString(DateFormat.FullMonthName);
}