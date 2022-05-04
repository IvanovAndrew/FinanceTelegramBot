using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GoogleSheet;

public class SheetOptions
{
    public string ListName;
    public string DateColumn;
    public string CategoryColumn;
    public string SubCategoryColumn;
    public string DescriptionColumn;
    public string AmountColumn;
}

public class GoogleSheetWriter
{
    private SheetOptions _options;
    private string _applicationName;
    private string _spreadsheetId;
    private UserCredential _credential;

    public GoogleSheetWriter(SheetOptions options, string applicationName, string spreadsheetId)
    {
        _options = options;
        _applicationName = applicationName;
        _spreadsheetId = spreadsheetId;
    }
    
    public Task WriteToSpreadsheet(IExpense expense, CancellationToken cancellationToken)
    {
        using (var stream =
            new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
        {
            // The file token.json stores the user's access and refresh tokens, and is created
            // automatically when the authorization flow completes for the first time.
            string credPath = "token.json";
            _credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                new [] { SheetsService.Scope.Spreadsheets },
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
        }

        // Create Google Sheets API service.
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _credential,
            ApplicationName = _applicationName,
        });

        int row = GetRow();

        // Define request parameters.
        string range = $"{_options.ListName}!{_options.DateColumn}{row}:{_options.AmountColumn}";
        SpreadsheetsResource.ValuesResource.GetRequest request =
            service.Spreadsheets.Values.Get(_spreadsheetId, range);

        await request.ExecuteAsync(cancellationToken);

    }
}