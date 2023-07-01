using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace EngineTest;

public class ExpenseRepositoryStub : IExpenseRepository
{
    private List<IExpense> _savedExpenses = new List<IExpense>();
    public Task Save(IExpense expense, CancellationToken cancellationToken)
    {
        return Task.Run(() => _savedExpenses.Add(expense));
    }

    public Task<List<IExpense>> Read(Predicate<DateOnly> dateFilter, CancellationToken cancellationToken)
    {
        return Task.Run(() => _savedExpenses.Where(c => dateFilter(c.Date)).ToList());
    }
}