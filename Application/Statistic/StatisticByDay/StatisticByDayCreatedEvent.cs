using MediatR;

namespace Application.Statistic.StatisticByDay;

public record StatisticByDayCreatedEvent : INotification
{
    public long SessionId { get; set; }
}