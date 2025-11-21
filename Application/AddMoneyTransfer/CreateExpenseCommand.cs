using MediatR;

namespace Application.AddMoneyTransfer;

public record CreateExpenseCommand : IRequest
{
    public long SessionID { get; init; }
}