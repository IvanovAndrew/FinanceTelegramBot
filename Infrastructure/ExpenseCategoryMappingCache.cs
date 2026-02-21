using Application;
using Application.Services;
using Domain;

namespace Infrastructure;

using Microsoft.Extensions.Caching.Memory;

public class ExpenseCategoryMappingCache(
    IMemoryCache cache,
    IFinanceRepository financeRepository
) : IExpenseCategoryMappingCache
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public async Task<IReadOnlyDictionary<string, ExpenseCategorizerResult>> Get(
        Currency currency,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"expense-category-mapping:{currency}";

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;

            var outcomes = await financeRepository.ReadOutcomes(
                new FinanceFilter { Currency = currency },
                cancellationToken);

            return outcomes
                .Where(o => !string.IsNullOrEmpty(o.Description))
                .DistinctBy(o => o.Description)
                .ToDictionary(
                    o => o.Description,
                    o => ExpenseCategorizerResult.Create(o.Category, o.SubCategory)
                );
        })!;
    }
}
