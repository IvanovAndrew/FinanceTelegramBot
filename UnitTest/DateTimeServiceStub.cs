using Infrastructure;

namespace UnitTest;

public class DateTimeServiceStub : IDateTimeService
{
    private readonly DateTime _now;

    public DateTimeServiceStub(DateTime now)
    {
        _now = now;
    }

    public DateOnly Today() => DateOnly.FromDateTime(_now);
    public DateTime Now() => _now;
}