using MediatR;

namespace Application.Events;

public record UserStartedEvent : INotification
{
    public long SessionID { get; init; }
}