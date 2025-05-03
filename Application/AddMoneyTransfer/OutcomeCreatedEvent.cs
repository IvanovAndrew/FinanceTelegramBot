using MediatR;

namespace Application.AddMoneyTransfer;

public class OutcomeCreatedEvent : INotification
{
    public long SessionID { get; init; }
}