using MediatR;

namespace Application.AddMoneyTransfer;

public class OutcomeDescriptionAddedEvent : INotification
{
    public long SessionId { get; init; }
}