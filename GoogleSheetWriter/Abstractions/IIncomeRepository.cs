namespace GoogleSheetWriter.Abstractions;

public interface IIncomeRepository
{
    Task<IReadOnlyList<MoneyTransfer>> Read(MoneyTransferSearchOption searchOptions, CancellationToken cancellationToken);
    Task Write(MoneyTransfer income, CancellationToken cancellationToken);
}