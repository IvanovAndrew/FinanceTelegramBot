using MediatR;

namespace Application.Statistic.StatisticByCategory;

public class StatisticByCategoryCreatedEvent : INotification
{
    public long SessionId { get; init; }
}