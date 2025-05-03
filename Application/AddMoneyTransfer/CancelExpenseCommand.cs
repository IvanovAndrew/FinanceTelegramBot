using MediatR;

namespace Application.AddExpense;

public class CancelExpenseCommand : IRequest
{
    public long SessionId { get; init; }
}