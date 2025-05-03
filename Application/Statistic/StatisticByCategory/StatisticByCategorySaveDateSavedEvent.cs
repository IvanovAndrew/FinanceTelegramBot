using MediatR;

namespace Application.Statistic.StatisticByCategory;

public class StatisticByCategorySaveDateSavedEvent : INotification
{
    public long SessionId { get; init; }
}