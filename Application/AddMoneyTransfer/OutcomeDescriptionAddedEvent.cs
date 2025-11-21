using MediatR;

namespace Application.AddMoneyTransfer;

public record OutcomeDescriptionAddedEvent : INotification
{
    public long SessionId { get; init; }
}