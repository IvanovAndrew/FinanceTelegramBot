using System.Globalization;
using Domain;

namespace Application;

public interface IDateTimeService
{
    protected CultureInfo CultureInfo { get; }
    DateOnly Today();
    DateTime Now();

    bool TryParseMonth(string text, out YearMonth month)
    {
        var result = TryParseDate(text, out var date);
        month = YearMonth.From(date);

        return result;
    }
    bool TryParseDate(string text, out DateOnly date)
    {
        if (string.Equals(text, "today", StringComparison.InvariantCultureIgnoreCase))
        {
            date = Today();
            return true;
        }
        else if (string.Equals(text, "yesterday", StringComparison.InvariantCultureIgnoreCase))
        {
            date = Today().AddDays(-1);
            return true;
        }

        if (DateOnly.TryParse(text, CultureInfo, DateTimeStyles.None, out date))
            return true;

        return false;
    }
    
    bool TryParseDateTime(string text, out DateTime date)
    {
        if (DateTime.TryParse(text, CultureInfo, DateTimeStyles.None, out date))
            return true;

        return false;
    }
}