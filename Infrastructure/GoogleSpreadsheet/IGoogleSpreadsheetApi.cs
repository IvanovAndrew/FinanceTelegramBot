using Refit;

namespace Infrastructure.GoogleSpreadsheet;

public interface IGoogleSpreadsheetApi
{
    [Post("/SaveIncome")]
    Task<HttpResponseMessage> SaveIncomeAsync([Body] GoogleSpreadsheetIncomeDto income, CancellationToken cancellationToken);

    [Post("/GetAllIncomes")]
    Task<List<GoogleSpreadsheetIncomeDto>> GetIncomesAsync([Body] string financeFilterJson, CancellationToken cancellationToken);

    [Post("/GetAllExpenses")]
    Task<List<GoogleSpreadsheetExpenseDto>> GetExpensesAsync([Body] string financeFilterJson, CancellationToken cancellationToken);

    [Post("/SaveExpense")]
    Task<HttpResponseMessage> SaveExpenseAsync([Body] GoogleSpreadsheetExpenseDto expense, CancellationToken cancellationToken);

    [Post("/SaveAllExpenses")]
    Task<HttpResponseMessage> SaveAllExpensesAsync([Body] GoogleSpreadsheetExpenseDto[] expenses, CancellationToken cancellationToken);
}