using MediatR;

namespace Application.Core.AddMoneyTransfer;

public record OutcomeCreatedEvent : INotification
{
    public long SessionId { get; init; }
}