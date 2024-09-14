using Domain;

namespace Infrastructure;

public interface IFinanseRepository
{
    Task<bool> SaveIncome(IIncome income, CancellationToken cancellationToken);
    Task<bool> SaveOutcome(IExpense expense, CancellationToken cancellationToken) =>
        SaveAllOutcomes(new List<IExpense>() { expense }, cancellationToken);

    Task<bool> SaveAllOutcomes(List<IExpense> expenses, CancellationToken cancellationToken);
    Task<List<IExpense>> ReadOutcomes(FinanseFilter finanseFilter, CancellationToken cancellationToken);
    Task<List<IIncome>> ReadIncomes(FinanseFilter finanseFilter, CancellationToken cancellationToken);
}

public class FinanseFilter
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public Currency? Currency { get; set; }
}
