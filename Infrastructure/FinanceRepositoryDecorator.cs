using System.Collections.Concurrent;
using Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public static class CacheKeys
{
    public const string AllExpenses = "1";
    public const string AllIncomes = "2";
}
    
public class FinanceRepositoryDecorator : IFinanceRepository
{
    private readonly IFinanceRepository _repository;
    private readonly ILogger _logger;
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly ConcurrentDictionary<object, SemaphoreSlim> _locks = new();
    
    public FinanceRepositoryDecorator(IFinanceRepository repository, ILogger<FinanceRepositoryDecorator> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> SaveIncome(IMoneyTransfer income, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"FinanceRepository is trying to save an income");
        
        bool result = await _repository.SaveIncome(income, cancellationToken);
        
        _logger.LogInformation($"FinanceRepository has got result {result}");
        
        // TODO maybe it makes sense to add the expense to the cache
        _logger.LogInformation("Remove all cached entries");
        _cache.Remove(CacheKeys.AllIncomes);

        return result;
    }

    public async Task<bool> SaveAllOutcomes(List<IMoneyTransfer> expense, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"ExpenseRepository is trying to save {expense.Count} expense(s)");
        
        bool result = await _repository.SaveAllOutcomes(expense, cancellationToken);
        
        _logger.LogInformation($"ExpenseRepository has got result {result}");
        
        // TODO maybe it makes sense to add the expense to the cache
        _logger.LogInformation("Remove all cached entries");
        _cache.Remove(CacheKeys.AllExpenses);

        return result;
    }

    public async Task<List<IMoneyTransfer>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        var cacheKey =
            $"DateFrom={financeFilter.DateFrom};DateTo={financeFilter.DateTo};Category={financeFilter.Category};Subcategory={financeFilter.Subcategory};Currency={financeFilter.Currency?.Name}";
        
        if (!_cache.TryGetValue(cacheKey, out List<IMoneyTransfer> items))
        {
            SemaphoreSlim mylock = _locks.GetOrAdd(CacheKeys.AllExpenses, k => new SemaphoreSlim(1, 1));
            await mylock.WaitAsync();
            try
            {
                if (!_cache.TryGetValue(CacheKeys.AllExpenses, out List<IMoneyTransfer> cachedItems))
                {
                    _logger.LogInformation("Loading expenses from the repository");
                    cachedItems = await _repository.ReadOutcomes(financeFilter, cancellationToken);
            
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

    public async Task<List<IMoneyTransfer>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        var cacheKey =
            $"DateFrom={financeFilter.DateFrom};DateTo={financeFilter.DateTo};Category={financeFilter.Category};Subcategory={financeFilter.Subcategory};Currency={financeFilter.Currency?.Name}";
        
        if (!_cache.TryGetValue(cacheKey, out List<IMoneyTransfer> items))
        {
            SemaphoreSlim mylock = _locks.GetOrAdd(CacheKeys.AllIncomes, k => new SemaphoreSlim(1, 1));
            await mylock.WaitAsync();
            try
            {
                if (!_cache.TryGetValue(CacheKeys.AllIncomes, out List<IMoneyTransfer> cachedItems))
                {
                    _logger.LogInformation("Loading incomes from the repository");
                    cachedItems = await _repository.ReadIncomes(financeFilter, cancellationToken);
            
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                    _cache.Set(CacheKeys.AllIncomes, cachedItems, cacheEntryOptions);
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
}

public class FinanceRepository : IFinanceRepository
{
    private readonly IGoogleSpreadsheetService _spreadsheetService;

    public FinanceRepository(IGoogleSpreadsheetService spreadsheetService)
    {
        _spreadsheetService = spreadsheetService;
    }

    public async Task<bool> SaveIncome(IMoneyTransfer income, CancellationToken cancellationToken)
    {
        return await _spreadsheetService.SaveIncome(income, cancellationToken);
    }

    public async Task<bool> SaveAllOutcomes(List<IMoneyTransfer> expenses, CancellationToken cancellationToken)
    {
        return await _spreadsheetService.SaveAllExpenses(expenses, cancellationToken);
    }

    public async Task<List<IMoneyTransfer>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        return await _spreadsheetService.GetExpenses(financeFilter, cancellationToken);
    }

    public async Task<List<IMoneyTransfer>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        return await _spreadsheetService.GetIncomes(financeFilter, cancellationToken);
    }
}