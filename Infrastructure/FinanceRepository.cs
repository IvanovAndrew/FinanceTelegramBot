using Domain;
using Infrastructure.GoogleSpreadsheet;

namespace Infrastructure;

public class FinanceRepository(IGoogleSpreadsheetService spreadsheetService) : IFinanceRepository
{
    public async Task<SaveResult> SaveIncome(Income income, CancellationToken cancellationToken)
    {
        return await spreadsheetService.SaveIncomeAsync(income, cancellationToken);
    }

    public async Task<SaveResult> SaveAllOutcomes(IReadOnlyCollection<Outcome> expenses, CancellationToken cancellationToken)
    {
        return await spreadsheetService.SaveAllExpensesAsync(expenses, cancellationToken);
    }

    public async Task<IReadOnlyList<Outcome>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        return await spreadsheetService.GetExpensesAsync(financeFilter, cancellationToken);
    }

    public async Task<IReadOnlyList<Income>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        return await spreadsheetService.GetIncomesAsync(financeFilter, cancellationToken);
    }

    public async Task<IReadOnlyList<CurrencyExchange>> ReadCurrencyExchanges(Currency currency, DateOnly dateFrom, DateOnly? dateTo, 
        CancellationToken cancellationToken)
    {
        return await spreadsheetService.GetCurrencyExchanges(currency, dateFrom, dateTo, cancellationToken);
    }
}
