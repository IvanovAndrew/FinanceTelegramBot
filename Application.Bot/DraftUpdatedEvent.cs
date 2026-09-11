using MediatR;

namespace Application.Bot;

public record DraftUpdatedEvent : INotification
{
    public long SessionId { get; init; }
}