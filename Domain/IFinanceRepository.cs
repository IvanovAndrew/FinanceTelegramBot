namespace Domain;

public interface IFinanceRepository
{
    Task<SaveResult> SaveIncome(Income income, CancellationToken cancellationToken);
    Task<SaveResult> SaveOutcome(Outcome expense, CancellationToken cancellationToken) =>
        SaveAllOutcomes(new List<Outcome>() { expense }, cancellationToken);

    Task<SaveResult> SaveAllOutcomes(IReadOnlyCollection<Outcome> expenses, CancellationToken cancellationToken);
    Task<IReadOnlyList<Outcome>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken);
    Task<IReadOnlyList<Income>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken);

    Task<SaveResult> Save(IMoneyTransfer transfer, CancellationToken cancellationToken)
    {
        return transfer.IsIncome ? SaveIncome((Income)transfer, cancellationToken) : SaveOutcome((Outcome) transfer, cancellationToken);
    }

    Task<IReadOnlyList<CurrencyExchange>> ReadCurrencyExchanges(FinanceFilter financeFilter,
        CancellationToken cancellationToken)
        => ReadCurrencyExchanges(financeFilter.Currency, financeFilter.DateFrom.Value, financeFilter.DateTo, cancellationToken);

    Task<IReadOnlyList<CurrencyExchange>>
        ReadCurrencyExchanges(Currency currency, DateOnly dateFrom, DateOnly? dateTo, CancellationToken cancellationToken);
}