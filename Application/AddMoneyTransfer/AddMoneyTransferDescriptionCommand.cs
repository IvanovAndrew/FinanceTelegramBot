using MediatR;

namespace Application.AddMoneyTransfer;

public class AddMoneyTransferDescriptionCommand : IRequest
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
    public string Description { get; init; }
}