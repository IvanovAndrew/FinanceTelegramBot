using MediatR;

namespace Application;

public record DraftUpdatedEvent : INotification
{
    public long SessionId { get; init; }
}