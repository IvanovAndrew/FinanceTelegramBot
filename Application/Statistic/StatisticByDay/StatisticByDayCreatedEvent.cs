using MediatR;

namespace Domain.Events;

public class StatisticByDayCreatedEvent : INotification
{
    public long SessionId { get; set; }
}