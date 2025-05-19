using GoogleSheetWriter;

public class GoogleServiceStub : IGoogleService
{
    public string? LastUpdatedRange { get; private set; }
    public List<IList<object>>? LastUpdatedValues { get; private set; }

    public IGrid? LastReturnedGrid { get; set; }

    public Task<IGrid> GetSheetAsync(string listName, GoogleRequestOptions options, CancellationToken cancellationToken)
    {
        return Task.FromResult(LastReturnedGrid ?? new GridStub());
    }

    public Task UpdateSheetAsync(string range, List<IList<object>> values, CancellationToken cancellationToken)
    {
        LastUpdatedRange = range;
        LastUpdatedValues = values;
        return Task.CompletedTask;
    }
}

public class GridStub : IGrid
{
    public IReadOnlyList<IGridData> Data { get; init; } = new List<IGridData>();
}

public class GridDataStub : IGridData
{
    public IReadOnlyList<IRowData> RowData { get; init; } = new List<IRowData>();
}

public class RowDataStub : IRowData
{
    public IReadOnlyDictionary<string, CellData> Cells { get; init; } = new Dictionary<string, CellData>();

    public bool ContainsValue(params string[] values)
    {
        return Cells.Values.Any(cell => cell.Filled && values.Contains(cell.Value));
    }
}
