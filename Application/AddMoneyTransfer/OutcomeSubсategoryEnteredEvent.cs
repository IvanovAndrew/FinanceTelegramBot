using MediatR;

namespace Application.AddMoneyTransfer;

public class OutcomeSubcategoryEnteredEvent : INotification
{
    public long SessionId { get; init; }
}