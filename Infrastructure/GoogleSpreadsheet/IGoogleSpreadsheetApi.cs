using Refit;

namespace Infrastructure.GoogleSpreadsheet;

public interface IGoogleSpreadsheetApi
{
    [Get("/HealthCheck")]
    Task<bool> HealthCheck(CancellationToken cancellationToken);
    
    [Post("/SaveIncome")]
    Task<HttpResponseMessage> SaveIncomeAsync([Body] GoogleSpreadsheetIncomeDto income, CancellationToken cancellationToken);

    [Get("/GetAllIncomes")]
    Task<List<GoogleSpreadsheetIncomeDto>> GetIncomesAsync(DateOnly? dateFrom, DateOnly? dateTo, string? category, string? currency, CancellationToken cancellationToken);

    [Get("/GetAllExpenses")]
    Task<List<GoogleSpreadsheetExpenseDto>> GetExpensesAsync(DateOnly? dateFrom, DateOnly? dateTo, string? category, string? subcategory, string? currency, CancellationToken cancellationToken);

    [Post("/SaveExpense")]
    Task<HttpResponseMessage> SaveExpenseAsync([Body] GoogleSpreadsheetExpenseDto expense, CancellationToken cancellationToken);

    [Post("/SaveAllExpenses")]
    Task<HttpResponseMessage> SaveAllExpensesAsync([Body] GoogleSpreadsheetExpenseDto[] expenses, CancellationToken cancellationToken);
    
    [Get("/GetCurrencyExchanges")]
    Task<List<GoogleSpreadsheetCurrencyExchangeDto>> GetCurrencyExchangesAsync(DateOnly? dateFrom, DateOnly? dateTo, string? currency, CancellationToken cancellationToken);
    
    [Get("/GetFutureExpenses")]
    Task<List<GoogleSpreadsheetFutureExpenseDto>> GetFutureExpensesAsync(string currency, CancellationToken cancellationToken);
}