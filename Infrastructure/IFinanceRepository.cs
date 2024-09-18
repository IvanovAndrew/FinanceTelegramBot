using Domain;

namespace Infrastructure;

public interface IFinanceRepository
{
    Task<bool> SaveIncome(IMoneyTransfer income, CancellationToken cancellationToken);
    Task<bool> SaveOutcome(IMoneyTransfer expense, CancellationToken cancellationToken) =>
        SaveAllOutcomes(new List<IMoneyTransfer>() { expense }, cancellationToken);

    Task<bool> SaveAllOutcomes(List<IMoneyTransfer> expenses, CancellationToken cancellationToken);
    Task<List<IMoneyTransfer>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken);
    Task<List<IMoneyTransfer>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken);
}

public class FinanceFilter
{
    public bool Income { get; init; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public Currency? Currency { get; set; }
}
