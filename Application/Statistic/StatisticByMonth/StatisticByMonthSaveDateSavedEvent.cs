using MediatR;

namespace Application.Statistic.StatisticByMonth;

public class StatisticByMonthSaveDateSavedEvent : INotification
{
    public long SessionId { get; init; }
}