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

    DateOnly FirstWorkingDayOfMonth(DateOnly day)
    {
        var firstWorkingDayOfMonth = new DateOnly(day.Year, day.Month, 1);

        return firstWorkingDayOfMonth.DayOfWeek switch
        {
            DayOfWeek.Saturday => firstWorkingDayOfMonth.AddDays(2),
            DayOfWeek.Sunday => firstWorkingDayOfMonth.AddDays(1),
            _ => firstWorkingDayOfMonth
        };
    }
}