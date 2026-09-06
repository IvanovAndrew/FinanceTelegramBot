using System.Net;
using GoogleSheetWriter;
using GoogleSheetWriter.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GoogleSpreadsheet;

public class GoogleSheetAzureFunction(IExpenseRepository expenseRepository,
    IIncomeRepository incomeRepository,
    ISheetRepository<CurrencyExchange> currencyExchangeRepository, 
    IFutureExpenseRepository futureExpenseRepository,
    ILogger<GoogleSheetAzureFunction> logger)
{
    [Function("GetAllExpenses")]
    public async Task<IActionResult> GetAllExpenses(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequest req,
        FunctionContext executionContext, CancellationToken cancellationToken)
    {
        var options = new MoneyTransferSearchOption
        {
            DateFrom = DateOnly.TryParse(req.Query["dateFrom"], out var dateFrom) ? dateFrom : null,
            DateTo = DateOnly.TryParse(req.Query["dateTo"], out var dateTo) ? dateTo : null,
            Category = req.Query["category"].ToString(),
            SubCategory = req.Query["subCategory"].ToString(),
            Currency = string.IsNullOrEmpty(req.Query["currency"]) ? null : req.Query["currency"].ToString()
        };

        logger.LogInformation($"Options are: " +
                              $"{(options.DateFrom != null? "DateFrom = " + options.DateFrom.Value : "")} " +
                              $"{(options.DateTo != null? "Date To = " + options.DateTo.Value : "")} " +
                              $"{(!string.IsNullOrEmpty(options.Category)? "Category is " + options.Category : "")} " +
                              $"{(!string.IsNullOrEmpty(options.SubCategory)? "Subcategory is " + options.SubCategory : "")} " +
                              $"{(options.Currency != null? "Currency is " + options.Currency : "")}");

        try
        {
            logger.LogInformation("Collecting expenses");
            var expenses = await expenseRepository.Read(options, cancellationToken);
            logger.LogInformation($"All {expenses.Count} expenses are successfully read");

            return new OkObjectResult(expenses.Select(Mapper.ToDto));
        }
        catch (Exception e)
        {
            logger.LogError("Couldn't read an expense: {e}", e);
            return new ObjectResult(e.ToString()) { StatusCode = 500 };
        }
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
            await expenseRepository.Write(new List<MoneyTransfer>() { expense }, cancellationToken);
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
            
            await expenseRepository.Write(expenses?.ToList()?? [], cancellationToken);
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
            await incomeRepository.Write(income, cancellationToken);
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
    public async Task<IActionResult> GetAllIncomes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequest req,
        FunctionContext executionContext, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Received a request body: {req.Body}");
        
        var options = new MoneyTransferSearchOption
        {
            DateFrom = DateOnly.TryParse(req.Query["dateFrom"], out var dateFrom) ? dateFrom : null,
            DateTo = DateOnly.TryParse(req.Query["dateTo"], out var dateTo) ? dateTo : null,
            Category = req.Query["category"].ToString(),
            SubCategory = req.Query["subCategory"].ToString(),
            Currency = string.IsNullOrEmpty(req.Query["currency"]) ? null : req.Query["currency"].ToString()
        };
        
        logger.LogInformation($"Options are: " +
                              $"{(options.DateFrom != null? "DateFrom = " + options.DateFrom.Value : "")} " +
                              $"{(options.DateTo != null? "Date To = " + options.DateTo.Value : "")} " + 
                              $"{(!string.IsNullOrEmpty(options.Category)? "Category is " + options.Category : "")} " + 
                              $"{(!string.IsNullOrEmpty(options.SubCategory)? "Subcategory is " + options.SubCategory : "")} " +
                              $"{(options.Currency != null? "Currency is " + options.Currency : "")}");
        
        try
        {
            logger.LogInformation("Collecting incomes");
            var incomes = await incomeRepository.Read(options, cancellationToken);
            logger.LogInformation($"All {incomes.Count} incomes are successfully read");
            
            return new OkObjectResult(incomes.Select(Mapper.ToDto));
        }
        catch (Exception e)
        {
            logger.LogError("Couldn't read an income: {e}", e);
            return new ObjectResult(e.ToString()) { StatusCode = 500 };
        }
    }
    
    [Function("GetCurrencyExchanges")]
    public async Task<IActionResult> GetCurrencyExchanges(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequest req,
        FunctionContext executionContext, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Received a request body: {req.Body}");
        
        var options = new MoneyTransferSearchOption
        {
            DateFrom = DateOnly.TryParse(req.Query["dateFrom"], out var dateFrom) ? dateFrom : null,
            DateTo = DateOnly.TryParse(req.Query["dateTo"], out var dateTo) ? dateTo : null,
            Currency = string.IsNullOrEmpty(req.Query["currency"]) ? null : req.Query["currency"].ToString()
        };
        
        logger.LogInformation($"Options are: " +
                              $"{(options.DateFrom != null? "DateFrom = " + options.DateFrom.Value : "")} " +
                              $"{(options.DateTo != null? "Date To = " + options.DateTo.Value : "")} " + 
                              $"{(options.Currency != null? "Currency is " + options.Currency : "")}");

        try
        {
            logger.LogInformation("Collecting currency exchanges");
            var exchanges = await currencyExchangeRepository.Read(options, cancellationToken);
            logger.LogInformation($"All {exchanges.Count} currency exchanges are successfully read");
            
            return new OkObjectResult(exchanges.Select(Mapper.ToCurrencyExchangeDto));
        }
        catch (Exception e)
        {
            logger.LogError("Couldn't read a currency exchange: {e}", e);
            return new ObjectResult(e.ToString()) { StatusCode = 500 };
        }
    }
    
    [Function("GetFutureExpenses")]
    public async Task<IActionResult> GetFutureExpenses(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequest req,
        FunctionContext executionContext, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Received a request body: {req.Body}");
        
        try
        {
            var currency = string.IsNullOrEmpty(req.Query["currency"]) ? string.Empty : req.Query["currency"].ToString();
            logger.LogInformation("Collecting future expenses for currency: {currency}", currency);
            
            var futureExpenses = await futureExpenseRepository.Read(currency, cancellationToken);
            logger.LogInformation($"All {futureExpenses.Count} future expenses are successfully read");

            return new OkObjectResult(futureExpenses.Select(Mapper.ToFutureExpenseDto));
        }
        catch (Exception e)
        {
            logger.LogError("Couldn't read future expenses}: {e}", e);
            return new ObjectResult(e.ToString()) { StatusCode = 500 };
        }
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