using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public record AddMoneyTransferByRequisitesCommand : IRequest
{
    public long SessionId { get; init; }
}