using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using Domain;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure;

public interface IGoogleSpreadsheetService
{
    public Task<bool> SaveIncome(IIncome income, CancellationToken cancellationToken);
    public Task<List<IIncome>> GetIncomes(FinanseFilter finanseFilter, CancellationToken cancellationToken);
    public Task<List<IExpense>> GetExpenses(FinanseFilter finanseFilter, CancellationToken cancellationToken);
    public Task<bool> SaveExpense(IExpense expense, CancellationToken cancellationToken);
    public Task<bool> SaveAllExpenses(List<IExpense> expenses, CancellationToken cancellationToken);
}

public class GoogleSpreadsheetService : IGoogleSpreadsheetService
{
    private readonly string _baseUrl;
    private readonly ILogger<IGoogleSpreadsheetService> _logger;
    private string GetExpensesUrl => $"{_baseUrl}/GetAllExpenses"; 
    private string SaveExpenseUrl => $"{_baseUrl}/SaveExpense"; 
    private string SaveAllExpensesUrl => $"{_baseUrl}/SaveAllExpenses";
    private string GetIncomesUrl => $"{_baseUrl}/GetAllIncomes";
    private string SaveIncomeUrl => $"{_baseUrl}/SaveIncome";
    public GoogleSpreadsheetService(string url, ILogger<IGoogleSpreadsheetService> logger)
    {
        _baseUrl = url;
        _logger = logger;
    }

    public async Task<bool> SaveIncome(IIncome income, CancellationToken cancellationToken)
    {
        using HttpClient httpClient = new HttpClient();
        using var jsonContent = JsonContent.Create(GoogleSpreadsheetIncomeDto.FromIncome(income));

        _logger.LogInformation($"Save the income. Json is {await jsonContent?.ReadAsStringAsync()}");
        var response = await httpClient.PostAsync(SaveIncomeUrl, jsonContent, cancellationToken);
        //var response = await httpClient.PostAsJsonAsync<GoogleSpreadsheetIncomeDto>(SaveIncomeUrl, GoogleSpreadsheetIncomeDto.FromIncome(income), cancellationToken);

        return response.StatusCode == HttpStatusCode.OK;
    }

    public async Task<List<IIncome>> GetIncomes(FinanseFilter finanseFilter, CancellationToken cancellationToken)
    {
        using HttpClient httpClient = new HttpClient();

        string jsonContent = JsonConvert.SerializeObject(
            new
            {
                finanseFilter.DateFrom,
                finanseFilter.DateTo,
                finanseFilter.Category,
                Currency = finanseFilter.Currency?.Name,
            });
        var content = new StringContent(jsonContent, Encoding.UTF8,  MediaTypeNames.Application.Json);
        
        _logger.LogInformation($"Getting expenses. Json is {await content.ReadAsStringAsync(cancellationToken)}");
        var response = await httpClient.PostAsJsonAsync(GetIncomesUrl, content, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var expensesString = await response.Content.ReadAsStringAsync(cancellationToken);
            var dtos = JsonConvert.DeserializeObject<List<GoogleSpreadsheetIncomeDto>>(expensesString);

            if (dtos == null)
                return new List<IIncome>();
                
            return dtos.Select(c => GoogleSpreadsheetIncomeDto.ToIncome(c)).ToList();
        }

        return new List<IIncome>();
    }

    public async Task<List<IExpense>> GetExpenses(FinanseFilter finanseFilter, CancellationToken cancellationToken)
    {
        using HttpClient httpClient = new HttpClient();

        string jsonContent = JsonConvert.SerializeObject(
            new
            {
                finanseFilter.DateFrom,
                finanseFilter.DateTo,
                finanseFilter.Category,
                finanseFilter.Subcategory,
                Currency = finanseFilter.Currency?.Name,
            });
        using var content = new StringContent(jsonContent, Encoding.UTF8,  MediaTypeNames.Application.Json);
        
        _logger.LogInformation($"Getting expenses. Json is {await content.ReadAsStringAsync(cancellationToken)}");
        var response = await httpClient.PostAsync(GetExpensesUrl, content, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var expensesString = await response.Content.ReadAsStringAsync(cancellationToken);
            var dtos = JsonConvert.DeserializeObject<List<GoogleSpreadsheetExpenseDto>>(expensesString);

            if (dtos == null)
                return new List<IExpense>();
                
            return dtos.Select(c => GoogleSpreadsheetExpenseDto.ToExpense(c)).ToList();
        }

        return new List<IExpense>();
    }

    public async Task<bool> SaveExpense(IExpense expense, CancellationToken cancellationToken)
    {
        using HttpClient httpClient = new HttpClient();
        using var jsonContent = JsonContent.Create(GoogleSpreadsheetExpenseDto.FromExpense(expense));

        _logger.LogInformation($"Save an expense. Json is {jsonContent?.Value}");
        var response = await httpClient.PostAsync(SaveExpenseUrl, jsonContent, cancellationToken);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> SaveAllExpenses(List<IExpense> expenses, CancellationToken cancellationToken)
    {
        try
        {
            using HttpClient httpClient = new HttpClient();
            
            string jsonContent = JsonConvert.SerializeObject(expenses.Select(GoogleSpreadsheetExpenseDto.FromExpense).ToArray());
            var content = new StringContent(jsonContent, Encoding.UTF8,  MediaTypeNames.Application.Json);

            var response = await httpClient.PostAsync(SaveAllExpensesUrl, content, cancellationToken);

            response.EnsureSuccessStatusCode();

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}

[Serializable]
public class GoogleSpreadsheetExpenseDto
{
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public static GoogleSpreadsheetExpenseDto FromExpense(IExpense expense)
    {
        return new GoogleSpreadsheetExpenseDto()
        {
            Date = expense.Date.ToDateTime(default),
            Category = expense.Category,
            Subcategory = expense.SubCategory,
            Description = expense.Description,
            Amount = expense.Amount.Amount,
            Currency = expense.Amount.Currency.Name
        };
    }

    public static IExpense ToExpense(GoogleSpreadsheetExpenseDto dto)
    {
        Domain.Currency currency;
        switch (int.Parse(dto.Currency))
        {
            case 0:
                currency = Domain.Currency.Rur;
                break;
            case 1:
                currency = Domain.Currency.Amd;
                break;
            case 2:
                currency = Domain.Currency.Gel;
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unknown currency code {dto.Currency}");
        }

        return new Expense()
        {
            Date = DateOnly.FromDateTime(dto.Date),
            Category = dto.Category,
            SubCategory = dto.Subcategory,
            Description = dto.Description,
            Amount = new Money()
            {
                Amount = dto.Amount,
                Currency = currency
            }
        };
    }
}

[Serializable]
public class GoogleSpreadsheetIncomeDto
{
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public static GoogleSpreadsheetIncomeDto FromIncome(IIncome income)
    {
        return new GoogleSpreadsheetIncomeDto()
        {
            Date = income.Date.ToDateTime(default),
            Category = income.Category,
            Description = income.Description,
            Amount = income.Amount.Amount,
            Currency = income.Amount.Currency.Name
        };
    }

    public static IIncome ToIncome(GoogleSpreadsheetIncomeDto dto)
    {
        Domain.Currency currency;
        switch (int.Parse(dto.Currency))
        {
            case 0:
                currency = Domain.Currency.Rur;
                break;
            case 1:
                currency = Domain.Currency.Amd;
                break;
            case 2:
                currency = Domain.Currency.Gel;
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unknown currency code {dto.Currency}");
        }

        return new Income()
        {
            Date = DateOnly.FromDateTime(dto.Date),
            Category = dto.Category,
            Description = dto.Description,
            Amount = new Money()
            {
                Amount = dto.Amount,
                Currency = currency
            }
        };
    }
}