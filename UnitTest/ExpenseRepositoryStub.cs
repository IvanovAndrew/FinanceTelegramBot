using Domain;
using Infrastructure;

namespace UnitTest;

public class ExpenseRepositoryStub : IExpenseRepository
{
    private readonly List<IExpense> _savedExpenses = new();
    public TimeSpan DelayTime { get; set; } = TimeSpan.Zero;

    public async Task<bool> SaveAll(List<IExpense> expenses, CancellationToken cancellationToken)
    {
        await Task.Delay(DelayTime, cancellationToken);
        _savedExpenses.AddRange(expenses);

        return true;
    }

    public Task<List<IExpense>> Read(CancellationToken cancellationToken)
    {
        return Task.Run(() => _savedExpenses);
    }
}