using System.Net;
using GoogleSheetWriter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SearchOption = GoogleSheetWriter.SearchOption;

namespace GoogleSpreadsheet;

public class GoogleSheetAzureFunction
{
    private readonly GoogleSheetWrapper _googleSheetWrapper;
    private readonly ILogger<GoogleSheetAzureFunction> _logger;

    public GoogleSheetAzureFunction(GoogleSheetWrapper googleSheetWrapper, ILogger<GoogleSheetAzureFunction> logger)
    {
        _googleSheetWrapper = googleSheetWrapper;
        _logger = logger;
    }
    
    [Function("GetAllExpenses")]
    public async Task<HttpResponseData> GetAllExpenses(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
        HttpRequestData req,
        FunctionContext executionContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Received a request body: {req.Body}");
        
        var request = await req.ReadAsStringAsync();
        _logger.LogInformation($"Received a string: {request}");

        var response = HttpResponseData.CreateResponse(req);
        
        try
        {
            SearchOption options;

            if (!string.IsNullOrEmpty(request))
            {
                options = JsonConvert.DeserializeObject<SearchOption>(request) ?? new SearchOption();
            }
            else
            {
                options = new SearchOption();
            }
            
            _logger.LogInformation($"Options are: " +
                                   $"{(options.DateFrom != null? "DateFrom = " + options.DateFrom.Value : "")} " +
                                   $"{(options.DateTo != null? "Date To = " + options.DateTo.Value : "")} " + 
                                   $"{(!string.IsNullOrEmpty(options.Category)? "Category is " + options.Category : "")} " + 
                                   $"{(!string.IsNullOrEmpty(options.SubCategory)? "Subcategory is " + options.SubCategory : "")} " +
                                   $"{(options.Currency != null? "Currency is " + options.Currency : "")}");
            
            _logger.LogInformation("Collecting expenses");
            var expenses = await _googleSheetWrapper.ReadExpenses(options, cancellationToken);
            _logger.LogInformation($"All {expenses.Count} expenses are successfully read");
            
            await response.WriteAsJsonAsync(expenses, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("Couldn't save an expense: {e}", e);
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync(e.ToString(), cancellationToken);
        }

        return response;
    }
    
    [Function("SaveExpense")]
    public async Task<HttpResponseData> SaveExpense(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext executionContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Received a request body: {req.Body}");
        
        var request = await req.ReadAsStringAsync();
        _logger.LogInformation($"Received a string: {request}");
        
        Expense expense = JsonConvert.DeserializeObject<Expense>(request);

        var response = HttpResponseData.CreateResponse(req);
        try
        {
            await _googleSheetWrapper.SaveAll(new List<Expense>() { expense }, cancellationToken);
            response.StatusCode = HttpStatusCode.OK;
            _logger.LogInformation("All expenses are successfully saved");
        }
        catch (Exception e)
        {
            _logger.LogError("Couldn't save an expense: {e}", e);
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync(e.ToString(), cancellationToken);
        }

        return response;
    }
    
    [Function("SaveAllExpenses")]
    public async Task<HttpResponseData> SaveAllExpenses(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext executionContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Received a request body: {req.Body}");
        
        var request = await req.ReadAsStringAsync();
        _logger.LogInformation($"Received a string: {request}");
        
        List<Expense> expenses = JsonConvert.DeserializeObject<List<Expense>>(request);
        _logger.LogInformation($"Deserialized as {expenses} Count: {expenses.Count}");

        var response = HttpResponseData.CreateResponse(req);
        try
        {
            await _googleSheetWrapper.SaveAll(expenses.ToList(), cancellationToken);
            response.StatusCode = HttpStatusCode.OK;
            _logger.LogInformation("All expenses are successfully saved");
        }
        catch (Exception e)
        {
            _logger.LogError("Couldn't save an expense: {e}", e);
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync(e.ToString(), cancellationToken);
        }

        return response;
    }
    
    [Function("SaveIncome")]
    public async Task<HttpResponseData> SaveIncome(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext executionContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Received a request body: {req.Body}");
        
        var request = await req.ReadAsStringAsync();
        _logger.LogInformation($"Received a string: {request}");
        
        Income income = JsonConvert.DeserializeObject<Income>(request);
        if (income == null)
        {
            _logger.LogError("Income is missing");
            return await BadRequestResponse(req, "Missing income");
        }

        var response = HttpResponseData.CreateResponse(req);
        try
        {
            await _googleSheetWrapper.SaveIncome(income, cancellationToken);
            response.StatusCode = HttpStatusCode.OK;
            _logger.LogInformation("The income are successfully saved");
        }
        catch (Exception e)
        {
            _logger.LogError("Couldn't save an Income: {e}", e);
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync(e.ToString(), cancellationToken);
        }

        return response;
    }
    
    [Function("GetAllIncomes")]
    public async Task<HttpResponseData> GetAllIncomes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
        HttpRequestData req,
        FunctionContext executionContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Received a request body: {req.Body}");
        
        var request = await req.ReadAsStringAsync();
        _logger.LogInformation($"Received a string: {request}");

        var response = HttpResponseData.CreateResponse(req);
        
        try
        {
            SearchOption options;

            if (!string.IsNullOrEmpty(request))
            {
                options = JsonConvert.DeserializeObject<SearchOption>(request) ?? new SearchOption();
            }
            else
            {
                options = new SearchOption();
            }
            
            _logger.LogInformation($"Options are: " +
                                   $"{(options.DateFrom != null? "DateFrom = " + options.DateFrom.Value : "")} " +
                                   $"{(options.DateTo != null? "Date To = " + options.DateTo.Value : "")} " + 
                                   $"{(!string.IsNullOrEmpty(options.Category)? "Category is " + options.Category : "")} " + 
                                   $"{(!string.IsNullOrEmpty(options.SubCategory)? "Subcategory is " + options.SubCategory : "")} " +
                                   $"{(options.Currency != null? "Currency is " + options.Currency : "")}");
            
            _logger.LogInformation("Collecting expenses");
            var expenses = await _googleSheetWrapper.ReadIncomes(options, cancellationToken);
            _logger.LogInformation($"All {expenses.Count} incomes are successfully read");
            
            await response.WriteAsJsonAsync(expenses, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("Couldn't read an income: {e}", e);
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync(e.ToString(), cancellationToken);
        }

        return response;
    }

    private async Task<HttpResponseData> BadRequestResponse(HttpRequestData req, string text)
    {
        return await ErrorResponse(req, HttpStatusCode.BadRequest, text);
    }
    
    private async Task<HttpResponseData> ErrorResponse(HttpRequestData req, HttpStatusCode code, string text)
    {
        var response = req.CreateResponse();
        response.StatusCode = code;
        await response.WriteStringAsync(text);
        return response;
    }
    
}