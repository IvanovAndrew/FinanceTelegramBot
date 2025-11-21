using MediatR;

namespace Application.AddMoneyTransfer;

public record MoneyTransferSavingStartedEvent : INotification
{
    public long SessionId { get; init; }
    public int MessageId { get; init; }
}