using MediatR;

namespace Application.AddMoneyTransfer;

public class CustomDateChosenEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
}