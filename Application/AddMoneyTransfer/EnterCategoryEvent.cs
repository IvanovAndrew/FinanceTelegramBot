using Domain;
using MediatR;

namespace Application.AddMoneyTransfer;

public record EnterCategoryEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
    public IReadOnlyList<Category> Categories { get; init; }
}