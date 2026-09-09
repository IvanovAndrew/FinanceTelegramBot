namespace GoogleSheetWriter.Infrastructure;

class GridData : IGridData
{
    public IReadOnlyList<IRowData> RowData { get; internal set; }

    public static IGridData FromGoogleGridData(Google.Apis.Sheets.v4.Data.GridData? gridData, string[] columns)
    {
        if (gridData?.RowData == null)
            return new GridData() { RowData = new List<IRowData>() };

        return new GridData() { RowData = gridData.RowData.Select(row => Infrastructure.RowData.FromGoogleRowData(row, columns)).ToList() };
    }
}