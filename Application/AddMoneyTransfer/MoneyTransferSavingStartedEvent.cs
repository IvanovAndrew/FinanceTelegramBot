using MediatR;

namespace Application.AddMoneyTransfer;

public class MoneyTransferSavingStartedEvent : INotification
{
    public long SessionId { get; init; }
    public int MessageId { get; init; }
}