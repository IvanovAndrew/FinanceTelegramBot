using System.Net;
using GoogleSheetWriter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GoogleSpreadsheet;

public class GoogleSheetAzureFunction(GoogleSheetWrapper googleSheetWrapper, ILogger<GoogleSheetAzureFunction> logger)
{
    [Function("GetAllExpenses")]
    public async Task<HttpResponseData> GetAllExpenses(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
        HttpRequestData req,
        FunctionContext executionContext, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Received a request body: {req.Body}");
        
        var request = await req.ReadAsStringAsync();
        logger.LogInformation($"Received a string: {request}");

        var response = HttpResponseData.CreateResponse(req);
        
        try
        {
            MoneyTransferSearchOption options;

            if (!string.IsNullOrEmpty(request))
            {
                options = JsonConvert.DeserializeObject<MoneyTransferSearchOption>(request) ?? new MoneyTransferSearchOption();
            }
            else
            {
                options = new MoneyTransferSearchOption();
            }
            
            logger.LogInformation($"Options are: " +
                                   $"{(options.DateFrom != null? "DateFrom = " + options.DateFrom.Value : "")} " +
                                   $"{(options.DateTo != null? "Date To = " + options.DateTo.Value : "")} " + 
                                   $"{(!string.IsNullOrEmpty(options.Category)? "Category is " + options.Category : "")} " + 
                                   $"{(!string.IsNullOrEmpty(options.SubCategory)? "Subcategory is " + options.SubCategory : "")} " +
                                   $"{(options.Currency != null? "Currency is " + options.Currency : "")}");
            
            logger.LogInformation("Collecting expenses");
            var expenses = await googleSheetWrapper.ReadExpenses(options, cancellationToken);
            logger.LogInformation($"All {expenses.Count} expenses are successfully read");
            
            await response.WriteAsJsonAsync(expenses, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError("Couldn't read an expense: {e}", e);
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
        logger.LogInformation($"Received a request body: {req.Body}");
        
        var request = await req.ReadAsStringAsync();
        logger.LogInformation($"Received a string: {request}");

        var response = HttpResponseData.CreateResponse(req);
        try
        {
            MoneyTransfer expense = JsonConvert.DeserializeObject<MoneyTransfer>(request);
            await googleSheetWrapper.SaveAll(new List<MoneyTransfer>() { expense }, cancellationToken);
            response.StatusCode = HttpStatusCode.OK;
            logger.LogInformation("All expenses are successfully saved");
        }
        catch (Exception e)
        {
            logger.LogError("Couldn't save an expense: {e}", e);
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
        logger.LogInformation($"Received a request body: {req.Body}");
        
        var request = await req.ReadAsStringAsync();
        logger.LogInformation($"Received a string: {request}");

        var response = HttpResponseData.CreateResponse(req);
        try
        {
            List<MoneyTransfer> expenses = JsonConvert.DeserializeObject<List<MoneyTransfer>>(request);
            logger.LogInformation($"Deserialized as {expenses} Count: {expenses?.Count}");
            
            await googleSheetWrapper.SaveAll(expenses?.ToList()?? [], cancellationToken);
            response.StatusCode = HttpStatusCode.OK;
            logger.LogInformation("All expenses are successfully saved");
        }
        catch (Exception e)
        {
            logger.LogError("Couldn't save an expense: {e}", e);
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
        logger.LogInformation($"Received a request body: {req.Body}");
        
        var request = await req.ReadAsStringAsync();
        logger.LogInformation($"Received a string: {request}");
        
        MoneyTransfer income = JsonConvert.DeserializeObject<MoneyTransfer>(request);
        if (income == null)
        {
            logger.LogError("Income is missing");
            return await BadRequestResponse(req, "Missing income");
        }

        var response = HttpResponseData.CreateResponse(req);
        try
        {
            await googleSheetWrapper.SaveIncome(income, cancellationToken);
            response.StatusCode = HttpStatusCode.OK;
            logger.LogInformation("The income are successfully saved");
        }
        catch (Exception e)
        {
            logger.LogError("Couldn't save an Income: {e}", e);
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
        logger.LogInformation($"Received a request body: {req.Body}");
        
        var request = await req.ReadAsStringAsync();
        logger.LogInformation($"Received a string: {request}");

        var response = HttpResponseData.CreateResponse(req);
        
        try
        {
            MoneyTransferSearchOption options;

            if (!string.IsNullOrEmpty(request))
            {
                options = JsonConvert.DeserializeObject<MoneyTransferSearchOption>(request) ?? new MoneyTransferSearchOption();
            }
            else
            {
                options = new MoneyTransferSearchOption();
            }
            
            logger.LogInformation($"Options are: " +
                                   $"{(options.DateFrom != null? "DateFrom = " + options.DateFrom.Value : "")} " +
                                   $"{(options.DateTo != null? "Date To = " + options.DateTo.Value : "")} " + 
                                   $"{(!string.IsNullOrEmpty(options.Category)? "Category is " + options.Category : "")} " + 
                                   $"{(!string.IsNullOrEmpty(options.SubCategory)? "Subcategory is " + options.SubCategory : "")} " +
                                   $"{(options.Currency != null? "Currency is " + options.Currency : "")}");
            
            logger.LogInformation("Collecting incomes");
            var expenses = await googleSheetWrapper.ReadIncomes(options, cancellationToken);
            logger.LogInformation($"All {expenses.Count} incomes are successfully read");
            
            await response.WriteAsJsonAsync(expenses, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError("Couldn't get an income: {e}", e);
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