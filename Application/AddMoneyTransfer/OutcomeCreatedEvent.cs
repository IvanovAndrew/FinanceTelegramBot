using MediatR;

namespace Application.AddMoneyTransfer;

public record OutcomeCreatedEvent : INotification
{
    public long SessionID { get; init; }
}