using MediatR;

namespace Application.AddMoneyTransfer;

public class MoneyTransferDateEnteredEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
}