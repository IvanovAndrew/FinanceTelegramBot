using System.Globalization;
using Domain;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;

namespace GoogleSheet;

public class SheetOptions
{
    public ListInfo UsualExpenses;

    public ListInfo FlatInfo;
}

public class ListInfo
{
    public string ListName;

    public string YearColumn;
    public string MonthColumn;
    public string DateColumn;
    public string CategoryColumn;
    public string SubCategoryColumn;
    public string DescriptionColumn;
    public string AmountRurColumn;
    public string AmountAmdColumn;
}

public class GoogleSheetWrapper
{
    private SheetOptions _options;
    private string _applicationName;
    private string _spreadsheetId;
    private GoogleCredential _credential;
    private CultureInfo _cultureInfo = new("ru-RU");

    public GoogleSheetWrapper(SheetOptions options, string applicationName, string spreadsheetId)
    {
        _options = options;
        _applicationName = applicationName;
        _spreadsheetId = spreadsheetId;
    }
    
    public async Task Write(IExpense expense, CancellationToken cancellationToken)
    {
        var service = await InitializeService(cancellationToken);

        var listInfo = _options.UsualExpenses;

        int row = await GetNumberFilledRows(service, listInfo.ListName, cancellationToken) + 1;
        cancellationToken.ThrowIfCancellationRequested();
        
        // Define request parameters.
        var money = expense.Amount;
        
        // TODO Now there is inner rule that columns follow one by one. It can't be true in general and can lead to issues
        string range = $"{listInfo.ListName}!{listInfo.YearColumn}{row}:{listInfo.AmountAmdColumn}{row}";
        var valueRange = new ValueRange
        {
            Range = range,
            MajorDimension = "ROWS",
            Values = new List<IList<object>>
            {
                new List<object>
                {
                    $"=YEAR({listInfo.DateColumn}{row})",
                    $"=MONTH({listInfo.DateColumn}{row})", 
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

    private async Task<SheetsService> InitializeService(CancellationToken cancellationToken)
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
        return service;
    }


    private async Task<int> GetNumberFilledRows(SheetsService service, string listName, CancellationToken cancellationToken)
    {
        // TODO call it Async
        var request = service.Spreadsheets.Get(_spreadsheetId);
        request.IncludeGridData = true;
        request.Ranges = $"{listName}!A1:B";
        var response = await request.ExecuteAsync(cancellationToken);
        var sheet = response.Sheets.First(s => s.Properties.Title == listName);

        int i = 0;
        foreach (var data in sheet.Data)
        {
            foreach (var rowData in data.RowData)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
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

    public async Task<List<Expense>> GetRows(DateOnly month, CancellationToken cancellationToken)
    {
        var service = await InitializeService(cancellationToken);

        var usualExpenses = await GetRows(service, _options.UsualExpenses, "", month, cancellationToken);
        var flatExpenses = await GetRows(service, _options.FlatInfo, "Квартира", month, cancellationToken);

        return usualExpenses.Union(flatExpenses).OrderBy(e => e.Date).ToList();
    }

    private async Task<List<Expense>> GetRows(SheetsService service, ListInfo info, string category, DateOnly monthAndYear,
        CancellationToken cancellationToken)
    {
        var request = service.Spreadsheets.Get(_spreadsheetId);
        request.IncludeGridData = true;
        request.Ranges = $"{info.ListName}!{info.DateColumn}1:{info.AmountAmdColumn}";
        var response = await request.ExecuteAsync(cancellationToken);
        var sheet = response.Sheets.First(s => s.Properties.Title == info.ListName);

        List<Expense> expenses = new List<Expense>();

        var dateIndex = 0;
        int? categoryIndex = string.IsNullOrEmpty(category) ? info.CategoryColumn[0] - info.DateColumn[0] : null;
        int? subCategoryIndex = !string.IsNullOrEmpty(info.SubCategoryColumn)? info.SubCategoryColumn[0] - info.DateColumn[0] : null;
        int? descriptionIndex = !string.IsNullOrEmpty(info.DescriptionColumn)? info.DescriptionColumn[0] - info.DateColumn[0] : null;
        int rurAmountIndex = info.AmountRurColumn[0] - info.DateColumn[0];
        int amdAmountIndex = info.AmountAmdColumn[0] - info.DateColumn[0];

        int i = 0;
        foreach (var data in sheet.Data)
        {
            foreach (var rowData in data.RowData)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                bool filled = false;
                if (DateTime.TryParse(rowData.Values[dateIndex].FormattedValue, _cultureInfo, DateTimeStyles.None, out var date) && date.Month == monthAndYear.Month && date.Year == monthAndYear.Year)
                {
                    expenses.Add(
                        new Expense
                        {
                            Date = DateOnly.FromDateTime(date),
                            Category = categoryIndex != null? rowData.Values[categoryIndex.Value].FormattedValue : category,
                            SubCategory = subCategoryIndex != null? rowData.Values[subCategoryIndex.Value].FormattedValue : null,
                            Description = descriptionIndex != null? rowData.Values[descriptionIndex.Value].FormattedValue : null,
                            Amount = 
                                new Money
                                {
                                    Currency = rowData.Values[rurAmountIndex].FormattedValue != null? Currency.Rur : Currency.Amd,
                                    Amount = decimal.TryParse(rowData.Values[rurAmountIndex].FormattedValue?? rowData.Values[amdAmountIndex].FormattedValue, out var amount)? amount : 0, 
                                }
                        });
                }
                i++;
            }
        }
        
        return expenses;
    }
}