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
    public Task<List<IExpense>> GetExpenses(ExpenseFilter expenseFilter, CancellationToken cancellationToken);
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
    public GoogleSpreadsheetService(string url, ILogger<IGoogleSpreadsheetService> logger)
    {
        _baseUrl = url;
        _logger = logger;
    }
    
    public async Task<List<IExpense>> GetExpenses(ExpenseFilter expenseFilter, CancellationToken cancellationToken)
    {
        using HttpClient httpClient = new HttpClient();

        string jsonContent = JsonConvert.SerializeObject(expenseFilter);
        var content = new StringContent(jsonContent, Encoding.UTF8,  MediaTypeNames.Application.Json);
        
        _logger.LogInformation($"Getting expenses. Json is {content}");
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
        var jsonContent = JsonContent.Create(GoogleSpreadsheetExpenseDto.FromExpense(expense));

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