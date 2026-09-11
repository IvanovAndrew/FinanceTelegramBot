using MediatR;

namespace Application.Core.AddMoneyTransfer;

public record CreateExpenseCommand : IRequest
{
    public long SessionId { get; init; }
}