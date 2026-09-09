using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter.Infrastructure;

public class GoogleService : IGoogleService
{
    private const string ManifestName = "GoogleSheetWriter.servicekey.json";

    private readonly string _applicationName;
    private readonly string _spreadsheetId;
    private readonly ILogger<GoogleService> _logger;
    private GoogleCredential? _credential;
    private SheetsService? _service;

    private SheetsService Service
    {
        get
        {
            if (_service != null)
            {
                return _service;
            }

            _logger.LogDebug("Initializing Google service");

            var assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream(ManifestName);
            using StreamReader reader = new StreamReader(stream);
            string serviceKey = reader.ReadToEnd();
            _credential = GoogleCredential.FromJson(serviceKey);

            _service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = _applicationName,
            });

            return _service;
        }
    }

    public GoogleService(string applicationName, string spreadsheetId, ILogger<GoogleService> logger)
    {
        _applicationName = applicationName;
        _spreadsheetId = spreadsheetId;
        _logger = logger;
    }

    public int RequestCount { get; private set; }

    public async Task<IGrid> GetSheetAsync(string listName, GoogleRequestOptions options,
        CancellationToken cancellationToken)
    {
        var request = Service.Spreadsheets.Get(_spreadsheetId);
        request.IncludeGridData = true;
        request.Ranges = options.Range;

        var response = await TrackedExecuteAsync(request, "GetSheetAsync", options.Range, cancellationToken);

        var sheet = response.Sheets.First(s => s.Properties.Title == listName);

        return Grid.FromGoogleSheet(sheet, options.RequestedColumns.Select(column => column.Name).ToArray());
    }

    public async Task<IReadOnlyDictionary<string, IGrid>> GetSheetsBatchAsync(
        IReadOnlyDictionary<string, GoogleRequestOptions> requests, CancellationToken cancellationToken)
    {
        if (requests.Count == 0)
            return new Dictionary<string, IGrid>();

        // to save order
        var keys = requests.Keys.ToList();

        var request = Service.Spreadsheets.Values.BatchGet(_spreadsheetId);
        request.Ranges = keys.Select(k => requests[k].Range).ToList();
        request.ValueRenderOption =
            SpreadsheetsResource.ValuesResource.BatchGetRequest.ValueRenderOptionEnum.FORMATTEDVALUE;
        request.MajorDimension = SpreadsheetsResource.ValuesResource.BatchGetRequest.MajorDimensionEnum.ROWS;

        var response = await TrackedExecuteAsync(request, "GetSheetsBatchAsync", $"{requests.Count} list(s)", cancellationToken);

        var result = new Dictionary<string, IGrid>();

        for (int i = 0; i < keys.Count; i++)
        {
            var listKey = keys[i];
            var valueRange = response.ValueRanges[i]; // BatchGet saves order
            var columns = requests[listKey].RequestedColumns.Select(c => c.Name).ToArray();
            result[listKey] = Grid.FromValueRange(valueRange, columns);
        }

        return result;
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

        await TrackedExecuteAsync(request, "UpdateSheetAsync", range, cancellationToken);
    }

    private Task<TResponse> TrackedExecuteAsync<TResponse>(
        ClientServiceRequest<TResponse> request, string methodName, string detail,
        CancellationToken cancellationToken)
    {
        RequestCount++;
        _logger.LogInformation("Google Sheets API request #{RequestCount} ({Method}, {Detail}).",
            RequestCount, methodName, detail);

        return request.ExecuteAsync(cancellationToken);
    }
}