using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public record StatisticBySubcategoryCreatedEvent : INotification
{
    public long SessionId { get; init; }
}