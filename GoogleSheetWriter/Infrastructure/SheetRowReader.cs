using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter.Infrastructure;

internal record SheetReadRequest<T>(
    string ListName,
    ExcelColumn FirstColumn,
    ExcelColumn LastColumn,
    DateRowResolver? DateRowResolver,
    DateOnly? DateFrom,
    Func<IReadOnlyDictionary<ExcelColumn, CellData>, T> RowFactory,
    Func<T, bool> IsSatisfied);

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
        var results = await ReadRowsBatch(
            new[] { new SheetReadRequest<T>(listName, firstColumn, lastColumn, dateRowResolver, dateFrom, rowFactory, isSatisfied) },
            cancellationToken);

        return results[listName];
    }

    internal async Task<IReadOnlyDictionary<string, List<T>>> ReadRowsBatch<T>(
        IReadOnlyList<SheetReadRequest<T>> requests,
        CancellationToken cancellationToken)
    {
        var results = requests.ToDictionary(r => r.ListName, _ => new List<T>());
        var byName = requests.ToDictionary(r => r.ListName);

        var cursors = requests.ToDictionary(r => r.ListName, r =>
        {
            int fromRow = 1;
            if (r.DateFrom != null && r.DateRowResolver != null)
            {
                fromRow = r.DateRowResolver.GetBestFirstRow(r.DateFrom.Value, 1);
            }
            return fromRow;
        });

        var active = new HashSet<string>(requests.Select(r => r.ListName));

        foreach (var listName in active)
        {
            _logger.LogInformation($"List: {listName} FromRange {cursors[listName]}.");
        }

        while (active.Count > 0)
        {
            var pageRequests = active.ToDictionary(
                listName => listName,
                listName =>
                {
                    var r = byName[listName];
                    var columns = ExcelColumn.ColumnsBetween(r.FirstColumn, r.LastColumn);
                    return new GoogleRequestOptions
                    {
                        Range = BuildRange(listName, r.FirstColumn, r.LastColumn, cursors[listName], BatchSize),
                        RequestedColumns = columns,
                    };
                });

            var response = await _googleService.GetSheetsBatchAsync(pageRequests, cancellationToken);

            foreach (var listName in active.ToList()) // копия — active меняется внутри цикла
            {
                var r = byName[listName];
                var sheet = response[listName];
                int rowsReturned = 0;
                bool listReachedEnd = false;

                foreach (var data in sheet.Data)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (var rowData in data.RowData)
                    {
                        rowsReturned++;

                        if (!IsRowFilled(rowData))
                        {
                            listReachedEnd = true;
                            break;
                        }

                        if (!rowData.Cells.Any()) continue;
                        if (rowData.ContainsValue("Дата", "Год", "Категория")) continue;

                        var cells = rowData.Cells.ToDictionary(k => ExcelColumn.FromString(k.Key), kvp => kvp.Value);
                        var row = r.RowFactory(cells);

                        if (r.IsSatisfied(row))
                        {
                            results[listName].Add(row);
                        }
                    }

                    if (listReachedEnd) break;
                }

                if (rowsReturned < BatchSize) listReachedEnd = true;

                if (listReachedEnd)
                {
                    active.Remove(listName);
                }
                else
                {
                    cursors[listName] += BatchSize;
                }
            }
        }
        
        return results;
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
                if (!IsRowFilled(rowData)) break;
                i++;
            }
        }

        return i;
    }

    private static bool IsRowFilled(IRowData rowData)
    {
        if (rowData.Cells == null) return false;
        return rowData.Cells.Values.Any(cellValue => cellValue.Filled);
    }

    internal string BuildRange(string listName, ExcelColumn firstColumn, ExcelColumn lastColumn, int startRow, int rowCount)
    {
        return $"{listName}!{firstColumn.Name}{startRow}:{lastColumn.Name}{startRow + rowCount - 1}";
    }
}