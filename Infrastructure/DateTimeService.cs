namespace Infrastructure;

public class DateTimeService : IDateTimeService
{
    public DateOnly Today()
    {
        return DateOnly.FromDateTime(DateTime.Today);
    }

    public DateTime Now() => DateTime.Now;
}