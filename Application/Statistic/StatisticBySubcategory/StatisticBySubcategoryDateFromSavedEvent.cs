using MediatR;

namespace Application.Statistic;

public class StatisticBySubcategoryDateFromSavedEvent : INotification
{
    public long SessionId { get; init; }
}