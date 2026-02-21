using Domain;

namespace Application.Services;

public interface IExpenseCategoryMappingCache
{
    Task<IReadOnlyDictionary<string, ExpenseCategorizerResult>> Get(
        Currency currency,
        CancellationToken cancellationToken);
}
