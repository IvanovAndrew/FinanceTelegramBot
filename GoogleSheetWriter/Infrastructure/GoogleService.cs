using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter;

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
        
        return Grid.FromGoogleSheet(sheet, options.RequestedColumns.Select(column => column.Name).ToArray());
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