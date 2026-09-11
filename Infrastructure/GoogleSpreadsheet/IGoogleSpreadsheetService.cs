using Domain;
using Domain.Services;

namespace Infrastructure.GoogleSpreadsheet;

public interface IGoogleSpreadsheetService
{
    Task<SaveResult> SaveIncomeAsync(IMoneyTransfer income, CancellationToken cancellationToken);
    Task<List<Income>> GetIncomesAsync(FinanceFilter financeFilter, CancellationToken cancellationToken);
    Task<List<Outcome>> GetExpensesAsync(FinanceFilter financeFilter, CancellationToken cancellationToken);
    Task<SaveResult> SaveExpenseAsync(Outcome expense, CancellationToken cancellationToken);
    Task<SaveResult> SaveAllExpensesAsync(IReadOnlyCollection<Outcome> expenses, CancellationToken cancellationToken);
    Task<List<CurrencyExchange>> GetCurrencyExchanges(Currency currency, DateOnly dateFrom, DateOnly? dateTo, CancellationToken cancellationToken);
    Task<List<RecurringExpenseDefinition>> GetFutureExpenses(Currency currency, CancellationToken cancellationToken);
}