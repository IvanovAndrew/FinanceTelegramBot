using System.Net;
using Domain;
using Domain.Services;
using Microsoft.Extensions.Logging;
using Refit;

namespace Infrastructure.GoogleSpreadsheet;

public class GoogleSpreadsheetService(
    IGoogleSpreadsheetApi googleSpreadsheetApi,
    ILogger<IGoogleSpreadsheetService> logger)
    : IGoogleSpreadsheetService
{
    private readonly IGoogleSpreadsheetApi _api = googleSpreadsheetApi ?? throw new ArgumentNullException(nameof(googleSpreadsheetApi));
    private readonly ILogger<IGoogleSpreadsheetService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string QuotaExceededMessage = "Google Sheets quota exceeded, try again later.";

    public Task<SaveResult> SaveIncomeAsync(IMoneyTransfer income, CancellationToken cancellationToken) =>
        ExecuteSaveAsync(async () =>
        {
            var incomeDto = GoogleSpreadsheetIncomeDto.FromIncome(income);
            var response = await _api.SaveIncomeAsync(incomeDto, cancellationToken);
            return await HandleResponseAsync(response, income.ToString(), cancellationToken);
        }, nameof(SaveIncomeAsync));

    public Task<List<Income>> GetIncomesAsync(FinanceFilter financeFilter, CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var dtos = await _api.GetIncomesAsync(financeFilter.DateFrom, financeFilter.DateTo,
                financeFilter.Category?.Name, financeFilter.Currency?.Name, cancellationToken);
            return dtos?.Select(GoogleSpreadsheetIncomeDto.ToIncome).ToList() ?? [];
        }, nameof(GetIncomesAsync));

    public Task<List<Outcome>> GetExpensesAsync(FinanceFilter financeFilter, CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var dtos = await _api.GetExpensesAsync(financeFilter.DateFrom, financeFilter.DateTo,
                financeFilter.Category?.Name, financeFilter.Subcategory?.Name, financeFilter.Currency?.Name,
                cancellationToken);
            return dtos?.Select(d => GoogleSpreadsheetExpenseDto.ToExpense(d, _logger)).ToList() ?? [];
        }, nameof(GetExpensesAsync));

    public Task<SaveResult> SaveExpenseAsync(Outcome expense, CancellationToken cancellationToken) =>
        ExecuteSaveAsync(async () =>
        {
            var expenseDto = GoogleSpreadsheetExpenseDto.FromExpense(expense);
            var response = await _api.SaveExpenseAsync(expenseDto, cancellationToken);
            return await HandleResponseAsync(response, expense.ToString(), cancellationToken);
        }, nameof(SaveExpenseAsync));

    public Task<SaveResult> SaveAllExpensesAsync(IReadOnlyCollection<Outcome> expenses,
        CancellationToken cancellationToken) =>
        ExecuteSaveAsync(async () =>
        {
            var dtos = expenses.Select(GoogleSpreadsheetExpenseDto.FromExpense).ToArray();

            if (dtos.Length == 0)
            {
                _logger.LogWarning("Attempted to save empty or null expense batch!");
                return SaveResult.Fail("No expenses to save.");
            }

            if (dtos.Any(dto => dto == null))
            {
                _logger.LogWarning("Found null element in DTO array!");
            }

            _logger.LogInformation("Saving batch of expenses. Count: {Count}", dtos.Length);

            var response = await _api.SaveAllExpensesAsync(dtos, cancellationToken);

            if (response.IsSuccessStatusCode)
                return SaveResult.Ok();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var message =
                $"Failed to save expenses batch. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {content}";
            _logger.LogWarning(message);
            return SaveResult.Fail(message);
        }, nameof(SaveAllExpensesAsync));

    public Task<List<CurrencyExchange>> GetCurrencyExchanges(Currency currency, DateOnly dateFrom, DateOnly? dateTo,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var dtos = await _api.GetCurrencyExchangesAsync(dateFrom, dateTo, currency?.Name, cancellationToken);
            return dtos?.Select(d => GoogleSpreadsheetCurrencyExchangeDto.ToCurrencyExchange(d, _logger)).ToList() ?? [];
        }, nameof(GetCurrencyExchanges));

    public Task<List<RecurringExpenseDefinition>> GetFutureExpenses(Currency currency,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var dtos = await _api.GetFutureExpensesAsync(currency.Name, cancellationToken);
            return dtos?.Select(d => GoogleSpreadsheetFutureExpenseDto.ToRecurringExpenseDefinition(d, _logger)).ToList() ?? [];
        }, nameof(GetFutureExpenses));

    private async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string operationName)
    {
        try
        {
            return await operation();
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning(ex, "Google Sheets rate limit exceeded during {Operation}.", operationName);
            throw new GoogleSpreadsheetServiceException(QuotaExceededMessage, ex, isTransient: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during {Operation}.", operationName);
            throw new GoogleSpreadsheetServiceException($"Failed during {operationName}.", ex, IsTransientError(ex));
        }
    }

    private async Task<SaveResult> ExecuteSaveAsync(Func<Task<SaveResult>> operation, string operationName)
    {
        try
        {
            return await operation();
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning(ex, "Google Sheets rate limit exceeded during {Operation}.", operationName);
            return SaveResult.Fail(QuotaExceededMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during {Operation}.", operationName);
            return SaveResult.Fail($"Unexpected error: {ex.Message}");
        }
    }

    private async Task<SaveResult> HandleResponseAsync(HttpResponseMessage response, string context,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return SaveResult.Ok();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var message =
            $"Failed to save {context}. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {responseContent}";

        _logger.LogWarning(message);
        return SaveResult.Fail(message);
    }

    private static bool IsTransientError(Exception ex) =>
        ex is HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests };
}