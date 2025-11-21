using MediatR;

namespace Application.AddMoneyTransfer;

public record AddMoneyTransferDateCommand : IRequest
{
    public long SessionId { get; init; }
    public string DateText { get; init; }
}