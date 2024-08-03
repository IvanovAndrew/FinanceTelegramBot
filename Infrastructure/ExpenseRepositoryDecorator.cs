using System.Collections.Concurrent;
using Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public static class CacheKeys
{
    public const string AllExpenses = "1";
}
    
public class ExpenseRepositoryDecorator : IExpenseRepository
{
    private readonly IExpenseRepository _repository;
    private readonly ILogger _logger;
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly ConcurrentDictionary<object, SemaphoreSlim> _locks = new();
    
    public ExpenseRepositoryDecorator(IExpenseRepository repository, ILogger<ExpenseRepositoryDecorator> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> SaveAll(List<IExpense> expense, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"ExpenseRepository is trying to save {expense.Count} expense(s)");
        
        bool result = await _repository.SaveAll(expense, cancellationToken);
        
        _logger.LogInformation($"ExpenseRepository has got result {result}");
        
        // TODO maybe it makes sense to add the expense to the cache
        _logger.LogInformation("Remove all cached entries");
        _cache.Remove(CacheKeys.AllExpenses);

        return result;
    }

    public async Task<List<IExpense>> Read(ExpenseFilter expenseFilter, CancellationToken cancellationToken)
    {
        var cacheKey =
            $"DateFrom={expenseFilter.DateFrom};DateTo={expenseFilter.DateTo};Category={expenseFilter.Category};Subcategory={expenseFilter.Subcategory};Currency={expenseFilter.Currency?.Name}";
        
        if (!_cache.TryGetValue(cacheKey, out List<IExpense> items))
        {
            SemaphoreSlim mylock = _locks.GetOrAdd(CacheKeys.AllExpenses, k => new SemaphoreSlim(1, 1));
            await mylock.WaitAsync();
            try
            {
                if (!_cache.TryGetValue(CacheKeys.AllExpenses, out List<IExpense> cachedItems))
                {
                    _logger.LogInformation("Loading expenses from the repository");
                    cachedItems = await _repository.Read(expenseFilter, cancellationToken);
            
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                    _cache.Set(CacheKeys.AllExpenses, cachedItems, cacheEntryOptions);
                    _logger.LogInformation($"{cachedItems.Count} saved to the cache");
                }
                else
                {
                    _logger.LogInformation($"{items.Count} expenses are taken from the cache");
                }
                items = cachedItems;
            }
            finally
            {
                mylock.Release();
            }
        }
        else
        {
            _logger.LogInformation($"{items.Count} expenses are taken from the cache");
        }

        return items;
    }
}

public class ExpenseRepository : IExpenseRepository
{
    private readonly IGoogleSpreadsheetService _spreadsheetService;

    public ExpenseRepository(IGoogleSpreadsheetService spreadsheetService)
    {
        _spreadsheetService = spreadsheetService;
    }
    
    public async Task<bool> SaveAll(List<IExpense> expenses, CancellationToken cancellationToken)
    {
        return await _spreadsheetService.SaveAllExpenses(expenses, cancellationToken);
    }

    public async Task<List<IExpense>> Read(ExpenseFilter expenseFilter, CancellationToken cancellationToken)
    {
        return await _spreadsheetService.GetExpenses(expenseFilter, cancellationToken);
    }
}