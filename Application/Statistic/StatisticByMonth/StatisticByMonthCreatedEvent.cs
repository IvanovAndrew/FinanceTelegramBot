using MediatR;

namespace Application.Statistic.StatisticByMonth;

public record StatisticByMonthCreatedEvent : INotification
{
    public long SessionId { get; init; }
}