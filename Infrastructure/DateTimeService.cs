using System.Globalization;
using Application;

namespace Infrastructure;

public class DateTimeService : IDateTimeService
{
    private CultureInfo _cultureInfo = new CultureInfo("ru-RU");
    
    CultureInfo IDateTimeService.CultureInfo => _cultureInfo;

    public DateOnly Today()
    {
        return DateOnly.FromDateTime(DateTime.Today);
    }

    public DateTime Now() => DateTime.Now;
}