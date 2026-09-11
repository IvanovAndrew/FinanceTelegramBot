using Application.Core;
using Application.Core.Services;
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
        Currency currency, DateOnly startFrom,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"expense-category-mapping:{currency}{startFrom}";

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;

            var outcomes = await financeRepository.ReadOutcomes(
                new FinanceFilter { Currency = currency, DateFrom = startFrom },
                cancellationToken);

            return outcomes
                .Where(o => !string.IsNullOrWhiteSpace(o.Description))
                .Select(o => new
                {
                    Key = o.Description.Trim(),
                    Value = ExpenseCategorizerResult.Create(o.Category, o.SubCategory)
                })
                .DistinctBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);
        })!;
    }
}
