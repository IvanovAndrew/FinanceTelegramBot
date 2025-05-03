using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryByMonthCreatedEvent : INotification
{
    public long SessionId { get; init; }
}