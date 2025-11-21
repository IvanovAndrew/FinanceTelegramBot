using MediatR;

namespace Application.AddMoneyTransfer;

public record IncomeCreatedEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}