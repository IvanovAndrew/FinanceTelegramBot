using MediatR;

namespace Application.AddMoneyTransfer;

public record OutcomeCategoryEnteredEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; } 
}