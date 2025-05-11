using MediatR;

namespace Application.AddMoneyTransfer;

public class IncomeCreatedEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}