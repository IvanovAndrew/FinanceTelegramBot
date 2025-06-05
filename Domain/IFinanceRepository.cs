namespace Domain;

public interface IFinanceRepository
{
    Task<SaveResult> SaveIncome(IMoneyTransfer income, CancellationToken cancellationToken);
    Task<SaveResult> SaveOutcome(IMoneyTransfer expense, CancellationToken cancellationToken) =>
        SaveAllOutcomes(new List<IMoneyTransfer>() { expense }, cancellationToken);

    Task<SaveResult> SaveAllOutcomes(IReadOnlyCollection<IMoneyTransfer> expenses, CancellationToken cancellationToken);
    Task<List<IMoneyTransfer>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken);
    Task<List<IMoneyTransfer>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken);
}