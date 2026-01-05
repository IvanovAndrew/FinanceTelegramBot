namespace Domain.Services;

public interface ISpendingDayPolicy
{
    bool CanInclude(DateTime now);
}

public class SpendingDayPolicy : ISpendingDayPolicy
{
    public bool CanInclude(DateTime now) => now.Hour <= 18;
}