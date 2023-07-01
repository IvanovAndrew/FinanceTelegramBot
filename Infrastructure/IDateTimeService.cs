using System.Globalization;

namespace Infrastructure;

public interface IDateTimeService
{
    DateOnly Today();

    bool TryParse(string text, out DateOnly date)
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
}