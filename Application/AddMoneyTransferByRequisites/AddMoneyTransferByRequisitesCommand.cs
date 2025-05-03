using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class AddMoneyTransferByRequisitesCommand : IRequest
{
    public long SessionId { get; init; }
}