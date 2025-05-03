using MediatR;

namespace Application.Events;

public class OutcomeSubсategoryEnteredEvent : INotification
{
    public long SessionId { get; init; }
}