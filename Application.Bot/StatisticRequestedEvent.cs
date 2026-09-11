using MediatR;

namespace Application.Core.Events;

public record StatisticRequestedEvent : INotification
{
    public long SessionId { get; init; }
}