using MediatR;

namespace Application.AddMoneyTransfer;

public class MoneyTransferIsNotSavedEvent : INotification
{
    public long SessionId { get; init; }
}