using Domain;

namespace Infrastructure.GoogleSpreadsheet;

public interface IGoogleSpreadsheetService
{
    Task<SaveResult> SaveIncomeAsync(IMoneyTransfer income, CancellationToken cancellationToken);
    Task<List<Income>> GetIncomesAsync(FinanceFilter financeFilter, CancellationToken cancellationToken);
    Task<List<Outcome>> GetExpensesAsync(FinanceFilter financeFilter, CancellationToken cancellationToken);
    Task<SaveResult> SaveExpenseAsync(IMoneyTransfer expense, CancellationToken cancellationToken);
    Task<SaveResult> SaveAllExpensesAsync(IReadOnlyCollection<IMoneyTransfer> expenses, CancellationToken cancellationToken);
}