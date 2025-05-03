using MediatR;

namespace Application.Statistic;

public class StatisticBySubcategoryCreatedEvent : INotification
{
    public long SessionId { get; init; }
}