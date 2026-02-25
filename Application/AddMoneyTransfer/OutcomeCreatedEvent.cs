using MediatR;

namespace Application.AddMoneyTransfer;

public record OutcomeCreatedEvent : INotification
{
    public long SessionId { get; init; }
}