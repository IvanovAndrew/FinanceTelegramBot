using Domain;
using Microsoft.Extensions.Logging;

namespace Infrastructure.GoogleSpreadsheet;

public class GoogleSpreadsheetService : IGoogleSpreadsheetService
{
    private readonly ICategoryProvider _categoryProvider;
    private readonly IGoogleSpreadsheetApi _api;
    private readonly ILogger<IGoogleSpreadsheetService> _logger;

    public GoogleSpreadsheetService(
        ICategoryProvider categoryProvider,
        IGoogleSpreadsheetApi googleSpreadsheetApi,
        ILogger<IGoogleSpreadsheetService> logger)
    {
        _categoryProvider = categoryProvider ?? throw new ArgumentNullException(nameof(categoryProvider));
        _api = googleSpreadsheetApi ?? throw new ArgumentNullException(nameof(googleSpreadsheetApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SaveResult> SaveIncomeAsync(IMoneyTransfer income, CancellationToken cancellationToken)
    {
        try
        {
            var incomeDto = GoogleSpreadsheetIncomeDto.FromIncome(income);

            var response = await _api.SaveIncomeAsync(incomeDto, cancellationToken);

            return await HandleResponseAsync(response, income.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving income.");
            return SaveResult.Fail($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<List<IMoneyTransfer>> GetIncomesAsync(FinanceFilter financeFilter,
        CancellationToken cancellationToken)
    {
        try
        {
            string jsonPayload = financeFilter.ToJsonPayload(false);
            _logger.LogInformation("Getting incomes with filter: {Filter}", jsonPayload);

            var dtos = await _api.GetIncomesAsync(jsonPayload, cancellationToken);
            return dtos?.Select(GoogleSpreadsheetIncomeDto.ToIncome).ToList() ?? new List<IMoneyTransfer>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting incomes.");
            return new List<IMoneyTransfer>();
        }
    }

    public async Task<List<IMoneyTransfer>> GetExpensesAsync(FinanceFilter financeFilter,
        CancellationToken cancellationToken)
    {
        try
        {
            string jsonPayload = financeFilter.ToJsonPayload(true);
            _logger.LogInformation("Getting expenses with filter: {Filter}", jsonPayload);

            var dtos = await _api.GetExpensesAsync(jsonPayload, cancellationToken);
            return dtos?.Select(dto => GoogleSpreadsheetExpenseDto.ToExpense(dto, _categoryProvider)).ToList() ??
                   new List<IMoneyTransfer>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting expenses.");
            return new List<IMoneyTransfer>();
        }
    }

    public async Task<SaveResult> SaveExpenseAsync(IMoneyTransfer expense, CancellationToken cancellationToken)
    {
        try
        {
            var expenseDto = GoogleSpreadsheetExpenseDto.FromExpense(expense);
            var response = await _api.SaveExpenseAsync(expenseDto, cancellationToken);

            return await HandleResponseAsync(response, expense.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving income.");
            return SaveResult.Fail($"Unexpected error: {ex}");
        }
    }

    public async Task<SaveResult> SaveAllExpensesAsync(IReadOnlyCollection<IMoneyTransfer> expenses,
        CancellationToken cancellationToken)
    {
        try
        {
            var dtos = expenses.Select(GoogleSpreadsheetExpenseDto.FromExpense).ToArray();

            if (dtos == null || dtos.Length == 0)
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving batch expenses.");
            return SaveResult.Fail($"Unexpected error: {ex}");
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
}