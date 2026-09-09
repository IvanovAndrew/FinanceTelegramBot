namespace GoogleSheetWriter.Infrastructure;

public interface IGoogleService
{
    int RequestCount { get; }
    Task<IGrid> GetSheetAsync(string listName, GoogleRequestOptions options, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<string, IGrid>> GetSheetsBatchAsync(
        IReadOnlyDictionary<string, GoogleRequestOptions> requests, CancellationToken cancellationToken);
    Task UpdateSheetAsync(string range, List<IList<object>> values, CancellationToken cancellationToken);
    
}

public interface IGrid
{
    IReadOnlyList<IGridData> Data { get; }
}

public interface IGridData
{
    IReadOnlyList<IRowData> RowData { get; }
}

public interface IRowData
{
    IReadOnlyDictionary<string, CellData> Cells { get; }
    bool ContainsValue(params string[] values);
}