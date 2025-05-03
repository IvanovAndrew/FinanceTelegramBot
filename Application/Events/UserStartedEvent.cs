using MediatR;

namespace Application.Events;

public class UserStartedEvent : INotification
{
    public long SessionID { get; init; }
}