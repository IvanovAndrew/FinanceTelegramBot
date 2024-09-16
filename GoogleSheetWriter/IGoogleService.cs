using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter;

public interface IGoogleService
{
    Task<IGrid> GetSheetAsync(string listName, GoogleRequestOptions options, CancellationToken cancellationToken);
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
    IReadOnlyDictionary<string, ICellData> Cells { get; }
    bool ContainsValue(params string[] values);
}

public interface ICellData
{
    bool Filled { get; }
    string? Value { get; }
}


public class GoogleRequestOptions
{
    internal string Range { get; init; }
    internal string[] RequestedColumns { get; init; }

    internal static string[] GetColumnsBetween(string firstColumn, string lastColumn)
    {
        if (string.IsNullOrEmpty(firstColumn) || string.IsNullOrEmpty(lastColumn))
            throw new ArgumentNullException("Columns are not specified");

        if (firstColumn.Length != 1 || lastColumn.Length != 1)
            throw new ArgumentOutOfRangeException("Now only 1-letter columns are supported");

        char start = firstColumn[0];
        char end = lastColumn[0];
        if (start > end)
        {
            (start, end) = (end, start);
            (firstColumn, lastColumn) = (lastColumn, firstColumn);
        }

        List<string> columns = new List<string>();
        columns.Add(firstColumn);

        // Проходим по буквам от start до end
        for (char c = (char)(start + 1); c < end; c++)
        {
            columns.Add(c.ToString());
        }
        
        columns.Add(lastColumn);

        return columns.ToArray();
    }
}

public class GoogleService : IGoogleService
{
    private readonly string _applicationName;
    private readonly string _spreadsheetId;
    private GoogleCredential? _credential;

    private SheetsService? _service;
    private SheetsService Service
    {
        get
        {
            if (_service != null)
            {
                _logger.LogDebug("Google service has already been initialized");
                return _service;
            }
            
            _logger.LogDebug("Initializing Google service");

            if (_credential == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
            
                using (Stream stream = assembly.GetManifestResourceStream("GoogleSheetWriter.servicekey.json"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string serviceKey = reader.ReadToEnd();
                    _credential = GoogleCredential.FromJson(serviceKey);
                }
            }

            // Create Google Sheets API service.
            _service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = _applicationName,
            });

            return _service;
        }
    }

    private readonly ILogger<GoogleService> _logger;

    public GoogleService(string applicationName, string spreadsheetId, ILogger<GoogleService> logger)
    {
        _applicationName = applicationName;
        _spreadsheetId = spreadsheetId;
        var assembly = Assembly.GetExecutingAssembly();
            
        using (Stream stream = assembly.GetManifestResourceStream("GoogleSheetWriter.servicekey.json"))
        using (StreamReader reader = new StreamReader(stream))
        {
            string serviceKey = reader.ReadToEnd();
            _credential = GoogleCredential.FromJson(serviceKey);
        }
        
        _service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _credential,
            ApplicationName = _applicationName,
        });
        
        _logger = logger;
    }
    
    public async Task<IGrid> GetSheetAsync(string listName, GoogleRequestOptions options,
        CancellationToken cancellationToken)
    {
        var request = Service.Spreadsheets.Get(_spreadsheetId);
        request.IncludeGridData = true;
        request.Ranges = options.Range;
        var response = await request.ExecuteAsync(cancellationToken);
        var sheet = response.Sheets.First(s => s.Properties.Title == listName);
        
        return Grid.FromGoogleSheet(sheet, options.RequestedColumns);
    }

    public async Task UpdateSheetAsync(string range, List<IList<object>> values, CancellationToken cancellationToken)
    {
        var valueRange = new ValueRange
        {
            Range = range,
            MajorDimension = "ROWS",
            Values = values
        };

        SpreadsheetsResource.ValuesResource.UpdateRequest request =
            Service.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        request.ValueInputOption =
            SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

        var result = await request.ExecuteAsync(cancellationToken);
    }
}

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
}

class GridData : IGridData
{
    public IReadOnlyList<IRowData> RowData { get; private set; }

    public static IGridData FromGoogleGridData(Google.Apis.Sheets.v4.Data.GridData? gridData, string[] columns)
    {
        if (gridData?.RowData == null)
            return new GridData() { RowData = new List<IRowData>() };

        return new GridData() { RowData = gridData.RowData.Select(row => GoogleSheetWriter.RowData.FromGoogleRowData(row, columns)).ToList() };
    }
}

class RowData : IRowData
{
    public IReadOnlyDictionary<string, ICellData> Cells { get; private set;}
    
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
            return new RowData() { Cells = new Dictionary<string, ICellData>() };

        if (rowData.Values.Count > columns.Length)
        {
            throw new InvalidOperationException(
                $"Columns count mismatch. Requested columns {string.Join(", ", columns)} but {string.Join(", ", rowData.Values)} were received");
        }

        var dictionary = new Dictionary<string, ICellData>();
        
        for (int i = 0; i < rowData.Values.Count; i++)
        {
            dictionary[columns[i]] = CellData.FromGoogleCellData(rowData.Values[i]);
        }

        return new RowData() { Cells = dictionary };
    }
}

class CellData : ICellData
{
    public bool Filled { get; private set; }
    public string? Value { get; private set;}

    internal static ICellData FromGoogleCellData(Google.Apis.Sheets.v4.Data.CellData? cellData)
    {
        if (cellData == null)
            return new CellData();
        
        return new CellData() { Filled = cellData.EffectiveValue != null, Value = cellData.FormattedValue};
    }
}