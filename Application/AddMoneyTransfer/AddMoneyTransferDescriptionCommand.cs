using MediatR;

namespace Application.AddMoneyTransfer;

public record AddMoneyTransferDescriptionCommand : IRequest
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
    public string Description { get; init; }
}