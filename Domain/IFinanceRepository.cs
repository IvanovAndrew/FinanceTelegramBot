namespace Domain;

public interface IFinanceRepository
{
    Task<SaveResult> SaveIncome(Income income, CancellationToken cancellationToken);
    Task<SaveResult> SaveOutcome(Outcome expense, CancellationToken cancellationToken) =>
        SaveAllOutcomes(new List<Outcome>() { expense }, cancellationToken);

    Task<SaveResult> SaveAllOutcomes(IReadOnlyCollection<Outcome> expenses, CancellationToken cancellationToken);
    Task<List<Outcome>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken);
    Task<List<Income>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken);

    Task<SaveResult> Save(IMoneyTransfer transfer, CancellationToken cancellationToken)
    {
        if (transfer.IsIncome)
            return SaveIncome((Income)transfer, cancellationToken);
        else 
            return SaveOutcome((Outcome) transfer, cancellationToken);
    }
}