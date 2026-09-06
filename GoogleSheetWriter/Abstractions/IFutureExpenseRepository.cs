using GoogleSheetWriter.Domain;

namespace GoogleSheetWriter.Abstractions;

public interface IFutureExpenseRepository
{
    Task<IReadOnlyList<FutureExpense>> Read(string currency, CancellationToken cancellationToken);
}