using MediatR;

namespace Application.Events;

public record CustomDateRequestedEvent : INotification
{
    public long SessionId { get; init; }
    public string Text { get; init; }
    public int? LastSentMessageId { get; init; }
}