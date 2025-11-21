using MediatR;

namespace Application.AddMoneyTransfer;

public record CancelExpenseCommand : IRequest
{
    public long SessionId { get; init; }
}