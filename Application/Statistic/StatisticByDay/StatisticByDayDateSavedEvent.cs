using MediatR;

namespace Application.Statistic.StatisticByDay;

public record StatisticByDayDateSavedEvent : INotification
{
    public long SessionId { get; init; }
}