namespace Domain.Services;

public interface IRecurringExpenseDefinitionsRepository
{
    Task<IReadOnlyList<RecurringExpenseDefinition>> GetActiveDefinitions(Currency currency, CancellationToken ct);
}