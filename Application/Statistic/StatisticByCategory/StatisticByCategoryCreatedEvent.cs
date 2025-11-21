using MediatR;

namespace Application.Statistic.StatisticByCategory;

public record StatisticByCategoryCreatedEvent : INotification
{
    public long SessionId { get; init; }
}