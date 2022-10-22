using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GoogleSheet;

public class SheetOptions
{
    public string ListName;
    public string MonthColumn;
    public string DateColumn;
    public string CategoryColumn;
    public string SubCategoryColumn;
    public string DescriptionColumn;
    public string AmountRurColumn;
    public string AmountAmdColumn;
}

public class GoogleSheetWriter
{
    private SheetOptions _options;
    private string _applicationName;
    private string _spreadsheetId;
    private GoogleCredential _credential;

    public GoogleSheetWriter(SheetOptions options, string applicationName, string spreadsheetId)
    {
        _options = options;
        _applicationName = applicationName;
        _spreadsheetId = spreadsheetId;
    }
    
    public async Task WriteToSpreadsheet(IExpense expense, CancellationToken cancellationToken)
    {
        await using (var stream =
            new FileStream("servicekey.json", FileMode.Open, FileAccess.Read))
        {
            // The file token.json stores the user's access and refresh tokens, and is created
            // automatically when the authorization flow completes for the first time.
            _credential = await GoogleCredential.FromStreamAsync(stream, cancellationToken);
        }

        // Create Google Sheets API service.
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _credential,
            ApplicationName = _applicationName,
        });

        cancellationToken.ThrowIfCancellationRequested();
        int row = await GetNumberFilledRows(service, _options.ListName, cancellationToken) + 1;
        cancellationToken.ThrowIfCancellationRequested();

        // Define request parameters.
        var money = expense.Amount;
        
        // TODO Now there is inner rule that columns follow one by one. It can't be true in general and can lead to issues
        string range = $"{_options.ListName}!{_options.MonthColumn}{row}:{_options.AmountAmdColumn}{row}";
        var valueRange = new ValueRange
        {
            Range = range,
            MajorDimension = "ROWS",
            Values = new List<IList<object>>
            {
                new List<object>
                {
                    $"=MONTH({_options.DateColumn}{row})", 
                    expense.Date.ToString("dd.MM.yyyy"), 
                    expense.Category, 
                    expense.SubCategory, 
                    expense.Description, 
                    money.Currency == Currency.Rur? money.Amount : "", 
                    money.Currency == Currency.Amd? money.Amount : ""
                },
            },
        };
        
        SpreadsheetsResource.ValuesResource.UpdateRequest request =
            service.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

        await request.ExecuteAsync(cancellationToken);
    }

    private async Task<int> GetNumberFilledRows(SheetsService service, string listName, CancellationToken cancellationToken)
    {
        // TODO call it Async
        var request = service.Spreadsheets.Get(_spreadsheetId);
        request.IncludeGridData = true;
        request.Ranges = $"{listName}!A1:G2000";
        var response = await request.ExecuteAsync(cancellationToken);
        var sheet = response.Sheets.First(s => s.Properties.Title == listName);

        int i = 0;
        foreach (var data in sheet.Data)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            foreach (var rowData in data.RowData)
            {
                bool filled = false;
                foreach (var cellValue in rowData.Values)
                {
                    if (cellValue.EffectiveValue != null)
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
}