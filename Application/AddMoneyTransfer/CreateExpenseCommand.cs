using MediatR;

namespace Application.AddExpense;

public class CreateExpenseCommand : IRequest
{
    public long SessionID { get; init; }
}