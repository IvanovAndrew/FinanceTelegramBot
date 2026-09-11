using MediatR;

namespace Application.Core.Events;

public record TaskCanceledEvent : INotification
{
    public long SessionId { get; init; }
}