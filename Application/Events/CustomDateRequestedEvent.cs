using MediatR;

namespace Application.Events;

public class CustomDateRequestedEvent : INotification
{
    public long SessionId { get; init; }
    public string Text { get; init; }
}