using MediatR;

namespace Application.Events;

public class TaskCanceledEvent : INotification
{
    public long SessionId { get; init; }
}