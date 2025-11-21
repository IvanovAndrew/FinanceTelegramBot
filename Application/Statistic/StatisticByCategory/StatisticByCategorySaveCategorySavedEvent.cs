using MediatR;

namespace Application.Statistic.StatisticByCategory;

public record StatisticByCategorySaveCategorySavedEvent : INotification
{
    public long SessionId { get; init; }
}