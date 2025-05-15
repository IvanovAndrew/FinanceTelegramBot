using MediatR;

namespace Application.AddMoneyTransfer;

public class OutcomePriceAddedEvent : INotification
{
    public long SessionId { get; init; }
}