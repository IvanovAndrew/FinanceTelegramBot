using MediatR;

namespace Application.AddMoneyTransfer;

public class AddMoneyTransferPriceCommand : IRequest
{
    public long SessionId { get; init; }
    public string Price { get; init; }
}