using MediatR;

namespace Application.Statistic.StatisticByCategory;

public class StatisticByCategorySaveCategorySavedEvent : INotification
{
    public long SessionId { get; init; }
}