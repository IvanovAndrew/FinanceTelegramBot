using System.Globalization;

namespace Application;

public interface IDateTimeService
{
    DateOnly Today();
    DateTime Now();

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

        if (DateOnly.TryParse(text, new CultureInfo("ru-RU"), DateTimeStyles.None, out date))
            return true;

        return false;
    }
    
    bool TryParseDateTime(string text, out DateTime date)
    {
        if (DateTime.TryParse(text, new CultureInfo("ru-RU"), DateTimeStyles.None, out date))
            return true;

        return false;
    }

    bool IsCurrentMonth(DateOnly date) => Today().Year == date.Year && Today().Month == date.Month;

    DateOnly GetFirstWorkingDayOfNextMonth()
    {
        var nextMonth = Today().AddMonths(1);
        var firstWorkingDayOfMonth = new DateOnly(nextMonth.Year, nextMonth.Month, 1);

        if (firstWorkingDayOfMonth.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            firstWorkingDayOfMonth = firstWorkingDayOfMonth.AddDays(1);
        }

        return firstWorkingDayOfMonth;
    }
}