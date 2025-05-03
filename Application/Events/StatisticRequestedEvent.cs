using MediatR;

namespace Application.Events;

public class StatisticRequestedEvent : INotification
{
    public long SessionId { get; init; }
}