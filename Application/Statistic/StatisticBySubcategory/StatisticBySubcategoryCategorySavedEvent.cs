using MediatR;

namespace Application.Statistic;

public class StatisticBySubcategoryCategorySavedEvent : INotification
{
    public long SessionId { get; init; }
}