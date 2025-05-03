using MediatR;

namespace Application.Events;

public class OutcomeCategoryEnteredEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; } 
}