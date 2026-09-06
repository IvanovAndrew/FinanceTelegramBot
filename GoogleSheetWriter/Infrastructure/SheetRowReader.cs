using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter.Infrastructure;

internal class SheetRowReader
{
    private const int BatchSize = 500;
    private readonly IGoogleService _googleService;
    private readonly ILogger _logger;

    internal SheetRowReader(IGoogleService googleService, ILogger logger)
    {
        _googleService = googleService;
        _logger = logger;
    }

    internal async Task<List<T>> ReadRows<T>(
        string listName,
        ExcelColumn firstColumn,
        ExcelColumn lastColumn,
        DateRowResolver? dateRowResolver,
        DateOnly? dateFrom,
        Func<IReadOnlyDictionary<ExcelColumn, CellData>, T> rowFactory,
        Func<T, bool> isSatisfied,
        CancellationToken cancellationToken)
    {
        List<T> rows = new();

        int fromRangeRow = 1;
        if (dateFrom != null && dateRowResolver != null)
        {
            fromRangeRow = dateRowResolver.GetBestFirstRow(dateFrom.Value, 1);
        }

        int lastFilledRow = await GetNumberFilledRows(listName, cancellationToken);

        ExcelColumn[] requestedColumns = ExcelColumn.ColumnsBetween(firstColumn, lastColumn);
        _logger.LogInformation($"List: {listName} FromRange {fromRangeRow} Last Filled Row = {lastFilledRow}.");

        while (fromRangeRow < lastFilledRow)
        {
            var sheet = await _googleService.GetSheetAsync(listName,
                new GoogleRequestOptions()
                {
                    Range = BuildRange(listName, firstColumn, lastColumn, fromRangeRow, BatchSize),
                    RequestedColumns = requestedColumns,
                }, cancellationToken);

            foreach (var data in sheet.Data)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var rowData in data.RowData)
                {
                    if (!rowData.Cells.Any()) continue;
                    if (rowData.ContainsValue("Дата", "Год", "Категория")) continue;

                    var cells = rowData.Cells.ToDictionary(k => ExcelColumn.FromString(k.Key), kvp => kvp.Value);

                    var row = rowFactory(cells);

                    if (isSatisfied(row))
                    {
                        rows.Add(row);
                    }
                }
            }

            fromRangeRow += BatchSize;
        }

        return rows;
    }

    internal async Task<int> GetNumberFilledRows(string listName, CancellationToken cancellationToken)
    {
        var sheet = await _googleService.GetSheetAsync(listName,
            new GoogleRequestOptions()
            {
                Range = $"{listName}!A1:B",
                RequestedColumns = new[] { "A", "B" }.Select(ExcelColumn.FromString).ToArray()
            }, cancellationToken);

        int i = 0;
        foreach (var data in sheet.Data)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var rowData in data.RowData)
            {
                bool filled = false;
                if (rowData.Cells == null) break;

                foreach (var cellValue in rowData.Cells.Values)
                {
                    if (cellValue.Filled)
                    {
                        filled = true;
                        break;
                    }
                }

                if (!filled) break;
                i++;
            }
        }

        return i;
    }

    internal string BuildRange(string listName, ExcelColumn firstColumn, ExcelColumn lastColumn, int startRow, int rowCount)
    {
        return $"{listName}!{firstColumn.Name}{startRow}:{lastColumn.Name}{startRow + rowCount - 1}";
    }
}