using System.Collections.Concurrent;
using Domain;
using Infrastructure.GoogleSpreadsheet;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public class FinanceRepositoryDecorator(IFinanceRepository repository, ICategoryProvider categoryProvider, ILogger<FinanceRepositoryDecorator> logger)
    : IFinanceRepository
{
    private readonly ILogger _logger = logger;
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private static readonly MemoryCacheEntryOptions DefaultCacheOptions =
        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
    private readonly ConcurrentDictionary<object, SemaphoreSlim> _locks = new();

    public async Task<SaveResult> SaveIncome(IMoneyTransfer income, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"FinanceRepository is trying to save an income");
        
        var result = await repository.SaveIncome(income, cancellationToken);
        
        _logger.LogInformation($"FinanceRepository has got result {result}");
        
        // TODO maybe it makes sense to add the expense to the cache
        _logger.LogInformation("Remove all cached entries");
        _cache.Clear();

        return result;
    }

    public async Task<SaveResult> SaveAllOutcomes(IReadOnlyCollection<IMoneyTransfer> expenses, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"ExpenseRepository is trying to save {expenses.Count} expense(s)");
        
        var saveResult = await repository.SaveAllOutcomes(expenses, cancellationToken);
        
        _logger.LogInformation($"ExpenseRepository has got result {saveResult}");
        
        // TODO maybe it makes sense to add the expense to the cache
        _logger.LogInformation("Remove all cached entries");
        _cache.Clear();

        return saveResult;
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
    
    private FinanceFilter ParseCacheKey(string cacheKey)
    {
        // Пример ключа:
        // "Outcome:DateFrom=2025-09-08;DateTo=2025-09-10;Category=Food;Subcategory=Snacks;Currency=Rur"

        var parts = cacheKey.Split(':', 2); // отделяем тип (Outcome/Income)
        if (parts.Length < 2)
            throw new ArgumentException("Invalid cache key format", nameof(cacheKey));

        var filters = parts[1].Split(';', StringSplitOptions.RemoveEmptyEntries);

        var financeFilter = new FinanceFilter();

        foreach (var filterPart in filters)
        {
            var kv = filterPart.Split('=', 2);
            if (kv.Length != 2) continue;

            var key = kv[0];
            var value = kv[1];

            switch (key)
            {
                case "DateFrom":
                    financeFilter.DateFrom = DateOnly.Parse(value);
                    break;
                case "DateTo":
                    financeFilter.DateTo = DateOnly.Parse(value);
                    break;
                case "Category":
                    financeFilter.Category = categoryProvider.GetCategoryByName(value, false);
                    break;
                case "Subcategory":
                    financeFilter.Subcategory = financeFilter.Category?.GetSubcategoryByName(value);
                    break;
                case "Currency":
                    financeFilter.Currency = Currency.TryParse(value, out var currency) ? currency : null;
                    break;
            }
        }

        return financeFilter;
    }

}

public class FinanceRepository(IGoogleSpreadsheetService spreadsheetService) : IFinanceRepository
{
    public async Task<SaveResult> SaveIncome(IMoneyTransfer income, CancellationToken cancellationToken)
    {
        return await spreadsheetService.SaveIncomeAsync(income, cancellationToken);
    }

    public async Task<SaveResult> SaveAllOutcomes(IReadOnlyCollection<IMoneyTransfer> expenses, CancellationToken cancellationToken)
    {
        return await spreadsheetService.SaveAllExpensesAsync(expenses, cancellationToken);
    }

    public async Task<List<IMoneyTransfer>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        return await spreadsheetService.GetExpensesAsync(financeFilter, cancellationToken);
    }

    public async Task<List<IMoneyTransfer>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        return await spreadsheetService.GetIncomesAsync(financeFilter, cancellationToken);
    }
}