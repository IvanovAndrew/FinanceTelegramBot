using MediatR;

namespace Domain.Events;

public class IncomeCreatedEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
}