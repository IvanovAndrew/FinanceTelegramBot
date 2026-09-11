using Domain;
using Domain.Services;

namespace Application.Test.Stubs;

public class RecurringExpenseDefinitionsRepositoryStub : IRecurringExpenseDefinitionsRepository
{
    public Task<IReadOnlyList<RecurringExpenseDefinition>> GetActiveDefinitions(Currency currency, CancellationToken ct)
    {
        return Task.FromResult<IReadOnlyList<RecurringExpenseDefinition>>(new List<RecurringExpenseDefinition>());
    }
}