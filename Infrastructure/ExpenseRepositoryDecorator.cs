﻿using System.Collections.Concurrent;
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

    public async Task SaveAll(List<IExpense> expense, CancellationToken cancellationToken)
    {
        await _repository.SaveAll(expense, cancellationToken);
        // TODO maybe it makes sense to add the expense to the cache
        _logger.LogInformation("Remove all cached entries");
        _cache.Remove(CacheKeys.AllExpenses);
    }

    public async Task<List<IExpense>> Read(CancellationToken cancellationToken)
    {
        if (!_cache.TryGetValue(CacheKeys.AllExpenses, out List<IExpense> items))
        {
            SemaphoreSlim mylock = _locks.GetOrAdd(CacheKeys.AllExpenses, k => new SemaphoreSlim(1, 1));
            await mylock.WaitAsync();
            try
            {
                if (!_cache.TryGetValue(CacheKeys.AllExpenses, out List<IExpense> cachedItems))
                {
                    _logger.LogInformation("Loading expenses from the repository");
                    cachedItems = await _repository.Read(cancellationToken);
            
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