using MediatR;

namespace Domain.Events;

public class StatisticByMonthCreatedEvent : INotification
{
    public long SessionId { get; init; }
}