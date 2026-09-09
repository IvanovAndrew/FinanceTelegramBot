namespace GoogleSheetWriter.Infrastructure;

class RowData : IRowData
{
    public IReadOnlyDictionary<string, CellData> Cells { get; private set;}
    
    public bool ContainsValue(params string[] values)
    {
        foreach (var cell in Cells.Values)
        {
            if (cell.Filled && values.Contains(cell.Value))
                return true;
        }

        return false;
    }

    internal static IRowData FromGoogleRowData(Google.Apis.Sheets.v4.Data.RowData? rowData, string[] columns)
    {
        if (rowData?.Values == null)
            return new RowData() { Cells = new Dictionary<string, CellData>() };

        if (rowData.Values.Count > columns.Length)
        {
            throw new InvalidOperationException(
                $"Columns count mismatch. Requested columns {string.Join(", ", columns)} but {string.Join(", ", rowData.Values.Select(v => v.FormattedValue))} were received");
        }

        var dictionary = new Dictionary<string, CellData>();
        
        for (int i = 0; i < rowData.Values.Count; i++)
        {
            dictionary[columns[i]] = CellDataHelper.FromGoogleCellData(rowData.Values[i]);
        }

        return new RowData() { Cells = dictionary };
    }

    internal static RowData FromValueRow(IList<object> row, string[] columns)
    {
        var dictionary = new Dictionary<string, CellData>();

        for (int i = 0; i < columns.Length; i++)
        {
            if (i < row.Count && row[i] != null)
            {
                dictionary[columns[i]] = new CellData { Filled = true, Value = row[i].ToString() };
            }
            else
            {
                dictionary[columns[i]] = new CellData { Filled = false, Value = null };
            }
        }

        return new RowData() { Cells = dictionary };
    }
}