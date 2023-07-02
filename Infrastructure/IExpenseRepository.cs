using Domain;

namespace Infrastructure;

public interface IExpenseRepository
{
    Task Save(IExpense expense, CancellationToken cancellationToken);
    Task<List<IExpense>> Read(CancellationToken cancellationToken);
}