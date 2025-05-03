namespace Domain;

public interface IFinanceRepository
{
    Task<bool> SaveIncome(IMoneyTransfer income, CancellationToken cancellationToken);
    Task<bool> SaveOutcome(IMoneyTransfer expense, CancellationToken cancellationToken) =>
        SaveAllOutcomes(new List<IMoneyTransfer>() { expense }, cancellationToken);

    Task<bool> SaveAllOutcomes(IReadOnlyCollection<IMoneyTransfer> expenses, CancellationToken cancellationToken);
    Task<List<IMoneyTransfer>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken);
    Task<List<IMoneyTransfer>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken);
}