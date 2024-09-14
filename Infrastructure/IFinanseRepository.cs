using Domain;

namespace Infrastructure;

public interface IFinanseRepository
{
    Task<bool> SaveIncome(IIncome income, CancellationToken cancellationToken);
    Task<bool> SaveOutcome(IExpense expense, CancellationToken cancellationToken) =>
        SaveAllOutcomes(new List<IExpense>() { expense }, cancellationToken);

    Task<bool> SaveAllOutcomes(List<IExpense> expenses, CancellationToken cancellationToken);
    Task<List<IExpense>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken);
    Task<List<IIncome>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken);
}

public class FinanceFilter
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public Currency? Currency { get; set; }
}
