using Domain;
using Domain.Services;
using Infrastructure.GoogleSpreadsheet;

namespace Infrastructure;

public class RecurringExpenseDefinitionsRepository(IGoogleSpreadsheetService googleSpreadsheetService) : IRecurringExpenseDefinitionsRepository
{
    public async Task<IReadOnlyList<RecurringExpenseDefinition>> GetActiveDefinitions(Currency currency, CancellationToken ct)
    {
        return await googleSpreadsheetService.GetFutureExpenses(currency, ct);
    }
}