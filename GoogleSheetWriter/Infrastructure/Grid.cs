using Google.Apis.Sheets.v4.Data;

namespace GoogleSheetWriter.Infrastructure;

class Grid : IGrid
{
    public IReadOnlyList<IGridData> Data { get; private set; }

    internal static Grid FromGoogleSheet(Sheet? sheet, string[] columns)
    {
        if (sheet?.Data == null)
            return new Grid(){Data = new List<IGridData>()};

        return new Grid()
        {
            Data = sheet.Data.Select(gridData => GridData.FromGoogleGridData(gridData, columns)).ToList()
        };
    }

    internal static Grid FromValueRange(ValueRange? valueRange, string[] columns)
    {
        if (valueRange?.Values == null)
            return new Grid() { Data = new List<IGridData>() };

        var rowDataList = valueRange.Values
            .Select(row => (IRowData)RowData.FromValueRow(row, columns))
            .ToList();

        return new Grid()
        {
            Data = new List<IGridData> { new GridData() { RowData = rowDataList } }
        };
    }
}