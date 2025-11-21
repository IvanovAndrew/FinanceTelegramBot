using MediatR;

namespace Application.Events;

public record StatisticRequestedEvent : INotification
{
    public long SessionId { get; init; }
}