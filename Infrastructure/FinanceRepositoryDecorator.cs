using System.Collections.Concurrent;
using Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class FinanceRepositoryDecorator(IFinanceRepository repository, ILogger<FinanceRepositoryDecorator> logger)
    : IFinanceRepository
{
    private readonly ILogger _logger = logger;
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly ConcurrentDictionary<object, SemaphoreSlim> _locks = new();

    private readonly ConcurrentDictionary<string, FinanceFilter> _cachedFilters = new();

    public async Task<SaveResult> SaveIncome(Income income, CancellationToken cancellationToken)
    {
        _logger.LogInformation("FinanceRepository is trying to save an income");
        var result = await repository.SaveIncome(income, cancellationToken);
        _logger.LogInformation($"FinanceRepository has got result {result}");

        _logger.LogInformation("Remove all cached entries");
        _cache.Clear();
        _cachedFilters.Clear();

        return result;
    }

    public async Task<SaveResult> SaveAllOutcomes(IReadOnlyCollection<Outcome> expenses, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"ExpenseRepository is trying to save {expenses.Count} expense(s)");
        var saveResult = await repository.SaveAllOutcomes(expenses, cancellationToken);
        _logger.LogInformation($"ExpenseRepository has got result {saveResult}");

        _logger.LogInformation("Remove all cached entries");
        _cache.Clear();
        _cachedFilters.Clear();

        return saveResult;
    }

    public Task<IReadOnlyList<Outcome>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken) =>
        GetOrLoad("Outcome", financeFilter, repository.ReadOutcomes, cancellationToken);

    public Task<IReadOnlyList<Income>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken) =>
        GetOrLoad("Income", financeFilter, repository.ReadIncomes, cancellationToken);

    public async Task<IReadOnlyList<CurrencyExchange>> ReadCurrencyExchanges(Currency currency, DateOnly dateFrom, DateOnly? dateTo, 
        CancellationToken cancellationToken)
    {
        var financeFilter = new FinanceFilter(){DateFrom = dateFrom, DateTo = dateTo, Currency = currency};
        var cacheKey = BuildCacheKey("CurrencyExchange", financeFilter);
        
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<CurrencyExchange> exact))
        {
            _logger.LogInformation($"CurrencyExchange: exact cache hit for {cacheKey}");
            return exact;
        }

        var result = await repository.ReadCurrencyExchanges(currency, dateFrom, dateTo, cancellationToken);

        if (result.Any())
        {
            SetCache(cacheKey, result, financeFilter);
        }

        return result;
    }

    private async Task<IReadOnlyList<T>> GetOrLoad<T>(
        string type,
        FinanceFilter filter,
        Func<FinanceFilter, CancellationToken, Task<IReadOnlyList<T>>> load,
        CancellationToken cancellationToken)
        where T : IMoneyTransfer
    {
        var cacheKey = BuildCacheKey(type, filter);

        if (_cache.TryGetValue(cacheKey, out List<T> exact))
        {
            _logger.LogInformation($"{type}: exact cache hit for {cacheKey}");
            return exact;
        }

        var broaderKey = _cachedFilters
            .Where(kv => kv.Key.StartsWith($"{type}:") && kv.Value.IsSupersetOf(filter))
            .Select(kv => kv.Key)
            .FirstOrDefault();

        if (broaderKey is not null && _cache.TryGetValue(broaderKey, out IReadOnlyList<T> broaderItems))
        {
            _logger.LogInformation($"{type}: serving {cacheKey} from broader cached entry {broaderKey}, no repository call");
            return broaderItems.Where(t => t.Matches(filter)).ToList();
        }

        var mylock = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await mylock.WaitAsync(cancellationToken);
        try
        {
            if (!_cache.TryGetValue(cacheKey, out IReadOnlyList<T> cachedItems))
            {
                _logger.LogInformation($"Loading {type.ToLower()}s from the repository for {cacheKey}");
                cachedItems = await load(filter, cancellationToken);
                
                if (cachedItems != null && cachedItems.Count > 0)
                {
                    SetCache(cacheKey, cachedItems, filter);
                    _logger.LogInformation($"{cachedItems.Count} {type.ToLower()}(s) saved to the cache");
                }
            }
            else
            {
                _logger.LogInformation($"{cachedItems.Count} {type.ToLower()}(s) are taken from the cache");
            }

            return cachedItems;
        }
        finally
        {
            mylock.Release();
        }
    }

    private void SetCache<T>(string key, IReadOnlyList<T> items, FinanceFilter filter)
    {
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
            .RegisterPostEvictionCallback((evictedKey, _, _, _) => _cachedFilters.TryRemove((string)evictedKey, out _));

        _cache.Set(key, items, options);
        _cachedFilters[key] = filter;
    }

    private string BuildCacheKey(string type, FinanceFilter filter) =>
        $"{type}:DateFrom={filter.DateFrom:yyyy-MM-dd};" +
        $"DateTo={filter.DateTo:yyyy-MM-dd};" +
        $"Category={filter.Category};" +
        $"Subcategory={filter.Subcategory};" +
        $"Currency={filter.Currency?.Name}";
}