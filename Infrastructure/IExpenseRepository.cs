using Domain;

namespace Infrastructure;

public interface IExpenseRepository
{
    Task<bool> Save(IExpense expense, CancellationToken cancellationToken) =>
        SaveAll(new List<IExpense>() { expense }, cancellationToken);

    Task<bool> SaveAll(List<IExpense> expenses, CancellationToken cancellationToken);
    Task<List<IExpense>> Read(ExpenseFilter expenseFilter, CancellationToken cancellationToken);
}

public class ExpenseFilter
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public Currency? Currency { get; set; }
}
