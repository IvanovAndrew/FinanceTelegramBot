using MediatR;

namespace Application.AddMoneyTransfer;

public record EnterThePriceEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
}