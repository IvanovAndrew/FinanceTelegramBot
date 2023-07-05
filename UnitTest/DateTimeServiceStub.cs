using Infrastructure;

namespace EngineTest;

public class DateTimeServiceStub : IDateTimeService
{
    private readonly DateOnly _today;

    public DateTimeServiceStub(DateTime today) : this(DateOnly.FromDateTime(today))
    {
        
    }
    
    public DateTimeServiceStub(DateOnly today)
    {
        _today = today;
    }

    public DateOnly Today() => _today;
}