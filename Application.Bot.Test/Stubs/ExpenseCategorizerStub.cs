using Application.Core;
using Application.Core.Services;
using Domain;

namespace Application.Test.Stubs;

public class ExpenseCategorizerStub : IExpenseCategorizer
{
    public ExpenseCategorizerResult? GetCategory(string title, IReadOnlyDictionary<string, ExpenseCategorizerResult> availableOptions)
    {
        return null;
    }
}

public class ExpenseCategoryMappingCacheStub : IExpenseCategoryMappingCache
{
    public Task<IReadOnlyDictionary<string, ExpenseCategorizerResult>> Get(Currency currency, DateOnly startDate, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyDictionary<string, ExpenseCategorizerResult>>(new Dictionary<string, ExpenseCategorizerResult>());
    }
}