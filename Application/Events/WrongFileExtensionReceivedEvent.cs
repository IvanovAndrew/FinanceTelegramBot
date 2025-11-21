using MediatR;

namespace Application.Events;

public record WrongFileExtensionReceivedEvent : INotification
{
    public long SessionId { get; init; }
}