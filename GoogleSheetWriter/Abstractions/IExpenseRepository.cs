namespace GoogleSheetWriter.Abstractions;

public interface IExpenseRepository
{
    Task<IReadOnlyList<MoneyTransfer>> Read(MoneyTransferSearchOption searchOptions, CancellationToken cancellationToken);
    Task Write(IReadOnlyList<MoneyTransfer> expenses, CancellationToken cancellationToken);
}