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

    public Task<List<IExpense>> Read(ExpenseFilter expenseFilter, CancellationToken cancellationToken)
    {
        var result = 
            _savedExpenses.Where(expense =>
                    (expenseFilter.DateFrom == null || expenseFilter.DateFrom.Value <= expense.Date) &&
                    (expenseFilter.DateTo == null || expense.Date <= expenseFilter.DateTo.Value) &&
                    (expenseFilter.Category == null || expenseFilter.Category == expense.Category) &&
                    (expenseFilter.Subcategory == null || expenseFilter.Subcategory == expense.SubCategory) &&
                    (expenseFilter.Currency == null || expenseFilter.Currency == expense.Amount.Currency)

                )
                .ToList();

        return Task.FromResult(result);
    }
}