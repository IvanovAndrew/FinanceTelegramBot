namespace GoogleSheetWriter.Infrastructure;

static class CellDataHelper
{
    internal static CellData FromGoogleCellData(Google.Apis.Sheets.v4.Data.CellData? cellData)
    {
        if (cellData == null)
            return new CellData();
        
        return new CellData() { Filled = cellData.EffectiveValue != null, Value = cellData.FormattedValue};
    }
}