using Domain;

namespace Infrastructure;

public interface IExpenseRepository
{
    Task Save(IExpense expense, CancellationToken cancellationToken) =>
        SaveAll(new List<IExpense>() { expense }, cancellationToken);
    Task SaveAll(List<IExpense> expense, CancellationToken cancellationToken);
    Task<List<IExpense>> Read(CancellationToken cancellationToken);
}