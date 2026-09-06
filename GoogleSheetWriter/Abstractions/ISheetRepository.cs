namespace GoogleSheetWriter.Abstractions;

public interface ISheetRepository<T>
{
    Task<IReadOnlyList<T>> Read(MoneyTransferSearchOption searchOptions, CancellationToken cancellationToken);
    Task Write(IReadOnlyList<T> items, CancellationToken cancellationToken);
}