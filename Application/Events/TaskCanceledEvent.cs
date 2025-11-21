using MediatR;

namespace Application.Events;

public record TaskCanceledEvent : INotification
{
    public long SessionId { get; init; }
}