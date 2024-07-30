using Domain;

namespace Infrastructure;

public interface IExpenseRepository
{
    Task<bool> Save(IExpense expense, CancellationToken cancellationToken) =>
        SaveAll(new List<IExpense>() { expense }, cancellationToken);

    Task<bool> SaveAll(List<IExpense> expenses, CancellationToken cancellationToken);
    Task<List<IExpense>> Read(CancellationToken cancellationToken);
}