using MediatR;

namespace Application.AddMoneyTransfer;

public class OutcomeCategoryEnteredEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; } 
}