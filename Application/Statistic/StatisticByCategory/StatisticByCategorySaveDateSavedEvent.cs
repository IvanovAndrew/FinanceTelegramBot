using MediatR;

namespace Application.Statistic.StatisticByCategory;

public record StatisticByCategorySaveDateSavedEvent : INotification
{
    public long SessionId { get; init; }
}