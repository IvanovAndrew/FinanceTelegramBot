using MediatR;

namespace Application.Events;

public class WrongFileExtensionReceivedEvent : INotification
{
    public long SessionId { get; init; }
}