using MediatR;

namespace Application.AddMoneyTransfer;

public record MoneyTransferDateEnteredEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
}