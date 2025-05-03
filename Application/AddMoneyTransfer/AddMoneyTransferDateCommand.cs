using MediatR;

namespace Application.AddMoneyTransfer;

public class AddMoneyTransferDateCommand : IRequest
{
    public long SessionId { get; init; }
    public string DateText { get; init; }
}