using Domain;

namespace Application.Core.Services;

public interface IExpenseCategoryMappingCache
{
    Task<IReadOnlyDictionary<string, ExpenseCategorizerResult>> Get(
        Currency currency,
        DateOnly startDate,
        CancellationToken cancellationToken);
}
