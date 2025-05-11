using Application;

namespace UnitTest.Stubs;

public class DateTimeServiceStub : IDateTimeService
{
    private DateTime _now;

    public DateTimeServiceStub(DateTime now)
    {
        _now = now;
    }

    public DateOnly Today() => DateOnly.FromDateTime(_now);
    public DateTime Now() => _now;

    public void SetToday(DateOnly dateOnly) => _now = dateOnly.ToDateTime(new TimeOnly());
}