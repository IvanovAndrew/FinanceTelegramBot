using MediatR;

namespace Application.AddMoneyTransfer;

public record EnterCategoryEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
    public bool IsIncome { get; init; }
}