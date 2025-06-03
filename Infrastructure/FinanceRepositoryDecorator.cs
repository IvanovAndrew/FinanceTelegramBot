using System.Collections.Concurrent;
using Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public class FinanceRepositoryDecorator(IFinanceRepository repository, ILogger<FinanceRepositoryDecorator> logger)
    : IFinanceRepository
{
    private readonly ILogger _logger = logger;
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private static readonly MemoryCacheEntryOptions DefaultCacheOptions =
        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
    private readonly ConcurrentDictionary<object, SemaphoreSlim> _locks = new();

    public async Task<bool> SaveIncome(IMoneyTransfer income, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"FinanceRepository is trying to save an income");
        
        bool result = await repository.SaveIncome(income, cancellationToken);
        
        _logger.LogInformation($"FinanceRepository has got result {result}");
        
        // TODO maybe it makes sense to add the expense to the cache
        _logger.LogInformation("Remove all cached entries");
        _cache.Clear();

        return result;
    }

    public async Task<bool> SaveAllOutcomes(IReadOnlyCollection<IMoneyTransfer> expense, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"ExpenseRepository is trying to save {expense.Count} expense(s)");
        
        bool result = await repository.SaveAllOutcomes(expense, cancellationToken);
        
        _logger.LogInformation($"ExpenseRepository has got result {result}");
        
        // TODO maybe it makes sense to add the expense to the cache
        _logger.LogInformation("Remove all cached entries");
        _cache.Clear();

        return result;
    }

    public async Task<List<IMoneyTransfer>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey("Outcome", financeFilter);
        
        _logger.LogInformation($"Finding cached version of key {cacheKey}");
        
        if (!_cache.TryGetValue(cacheKey, out List<IMoneyTransfer> items))
        {
            _logger.LogInformation("Cache key is not found");
            
            SemaphoreSlim mylock = _locks.GetOrAdd(cacheKey, k => new SemaphoreSlim(1, 1));
            await mylock.WaitAsync(cancellationToken);
            try
            {
                if (!_cache.TryGetValue(cacheKey, out List<IMoneyTransfer> cachedItems))
                {
                    _logger.LogInformation("Loading expenses from the repository");
                    
                    cachedItems = await repository.ReadOutcomes(financeFilter, cancellationToken);
                    _cache.Set(cacheKey, cachedItems, DefaultCacheOptions);

                    _logger.LogInformation($"{cachedItems.Count} saved to the cache");
                }
                else
                {
                    _logger.LogInformation($"{cachedItems.Count} expenses are taken from the cache");
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

    public async Task<List<IMoneyTransfer>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey("Income", financeFilter);
        
        if (!_cache.TryGetValue(cacheKey, out List<IMoneyTransfer> items))
        {
            SemaphoreSlim mylock = _locks.GetOrAdd(cacheKey, k => new SemaphoreSlim(1, 1));
            await mylock.WaitAsync(cancellationToken);
            try
            {
                if (!_cache.TryGetValue(cacheKey, out List<IMoneyTransfer> cachedItems))
                {
                    _logger.LogInformation("Loading incomes from the repository");
                    
                    cachedItems = await repository.ReadIncomes(financeFilter, cancellationToken);
            
                    _cache.Set(cacheKey, cachedItems, DefaultCacheOptions);
                    
                    _logger.LogInformation($"{cachedItems.Count} saved to the cache");
                }
                else
                {
                    _logger.LogInformation($"{items?.Count ?? 0} incomes are taken from the cache");
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
            _logger.LogInformation($"{items.Count} incomes are taken from the cache");
        }

        return items;
    }
    
    private string BuildCacheKey(string type, FinanceFilter filter)
    {
        return $"{type}:DateFrom={filter.DateFrom:yyyy-MM-dd};" +
               $"DateTo={filter.DateTo:yyyy-MM-dd};" +
               $"Category={filter.Category};" +
               $"Subcategory={filter.Subcategory};" +
               $"Currency={filter.Currency?.Name}";
    }
}

public class FinanceRepository(IGoogleSpreadsheetService spreadsheetService) : IFinanceRepository
{
    public async Task<bool> SaveIncome(IMoneyTransfer income, CancellationToken cancellationToken)
    {
        return await spreadsheetService.SaveIncome(income, cancellationToken);
    }

    public async Task<bool> SaveAllOutcomes(IReadOnlyCollection<IMoneyTransfer> expenses, CancellationToken cancellationToken)
    {
        return await spreadsheetService.SaveAllExpenses(expenses, cancellationToken);
    }

    public async Task<List<IMoneyTransfer>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        return await spreadsheetService.GetExpenses(financeFilter, cancellationToken);
    }

    public async Task<List<IMoneyTransfer>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        return await spreadsheetService.GetIncomes(financeFilter, cancellationToken);
    }
}