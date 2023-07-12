using Domain;
using Infrastructure;

namespace UnitTest;

public class ExpenseRepositoryStub : IExpenseRepository
{
    private List<IExpense> _savedExpenses = new();

    public Task SaveAll(List<IExpense> expenses, CancellationToken cancellationToken)
    {
        return Task.Run(() => _savedExpenses.AddRange(expenses));
    }

    public Task<List<IExpense>> Read(CancellationToken cancellationToken)
    {
        return Task.Run(() => _savedExpenses);
    }
}