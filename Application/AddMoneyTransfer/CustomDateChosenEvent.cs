using MediatR;

namespace Application.AddMoneyTransfer;

public record CustomDateChosenEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
}