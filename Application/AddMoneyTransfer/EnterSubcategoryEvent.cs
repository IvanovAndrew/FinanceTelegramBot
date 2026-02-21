using Domain;
using MediatR;

namespace Application.AddMoneyTransfer;

public record EnterSubcategoryEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
    public Category Category { get; init; }
}