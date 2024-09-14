using System.Globalization;
using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter
{
    public class GoogleSheetWrapper
    {
        private readonly SheetOptions _options;
        private readonly CategoryToListMappingOptions _categoryMapping;
        private readonly string _applicationName;
        private readonly string _spreadsheetId;
        private readonly ILogger<GoogleSheetWrapper> _logger;
        private GoogleCredential _credential;
        private readonly CultureInfo _cultureInfo = new("ru-RU");
        private const int BatchSize = 500;
        private char FirstExcelColumn = 'A';
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public GoogleSheetWrapper(SheetOptions options, CategoryToListMappingOptions mappingOptions,
            string applicationName, string spreadsheetId, ILogger<GoogleSheetWrapper> logger)
        {
            _options = options;
            _categoryMapping = mappingOptions;
            _applicationName = applicationName;
            _spreadsheetId = spreadsheetId;
            _logger = logger;
        }

        public async Task SaveAll(List<Expense> expenses, CancellationToken cancellationToken)
        {
            var service = await InitializeService(cancellationToken);

            if (!_categoryMapping.CategoryToList.TryGetValue(expenses[0].Category, out string listName))
            {
                listName = _categoryMapping.DefaultCategory;
            }

            ListInfo listInfo = _options.EveryDayExpenses;
            if (listName == _options.FlatInfo.ListName)
            {
                listInfo = _options.FlatInfo;
            }
            else if (listName == _options.BigDealInfo.ListName)
            {
                listInfo = _options.BigDealInfo;
            }
            else if (listName == _options.CurrencyConversion.ListName)
            {
                listInfo = _options.CurrencyConversion;
            }

            await _semaphore.WaitAsync(TimeSpan.FromMinutes(1), cancellationToken);
            try
            {
                int row = await GetNumberFilledRows(service, listInfo.ListName, cancellationToken) + 1;
                cancellationToken.ThrowIfCancellationRequested();

                // TODO Now there is inner rule that columns follow one by one. It can't be true in general and can lead to issues
                string range = $"{listInfo.ListName}!{listInfo.YearColumn}{row}:{listInfo.AmountGelColumn}{row + expenses.Count - 1}";

                var excelRowValues = FillExcelRows(expenses, listInfo, row);

                var valueRange = new ValueRange
                {
                    Range = range,
                    MajorDimension = "ROWS",
                    Values = excelRowValues
                };

                SpreadsheetsResource.ValuesResource.UpdateRequest request =
                    service.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
                request.ValueInputOption =
                    SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

                await request.ExecuteAsync(cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveIncome(Income income, CancellationToken cancellationToken)
        {
            var service = await InitializeService(cancellationToken);
            
            ListInfo listInfo = _options.Incomes;

            await _semaphore.WaitAsync(TimeSpan.FromMinutes(1), cancellationToken);
            try
            {
                int row = await GetNumberFilledRows(service, listInfo.ListName, cancellationToken) + 1;
                cancellationToken.ThrowIfCancellationRequested();

                // TODO Now there is inner rule that columns follow one by one. It can't be true in general and can lead to issues
                string range = $"{listInfo.ListName}!{listInfo.YearColumn}{row}:{listInfo.AmountGelColumn}{row}";

                var excelRowValues = FillExcelRows(income, listInfo, row);

                var valueRange = new ValueRange
                {
                    Range = range,
                    MajorDimension = "ROWS",
                    Values = excelRowValues
                };

                SpreadsheetsResource.ValuesResource.UpdateRequest request =
                    service.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
                request.ValueInputOption =
                    SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

                await request.ExecuteAsync(cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private List<IList<object>> FillExcelRows(Income income, ListInfo listInfo, int firstRow)
        {
            var excelRowValues = new List<object>();
            var row = firstRow;

            for (int j = 0; j < 10; j++)
            {
                excelRowValues.Add(null);
            }

            if (!string.IsNullOrEmpty(listInfo.YearColumn))
            {
                excelRowValues[listInfo.YearColumn[0] - FirstExcelColumn] = $"=YEAR({listInfo.DateColumn}{row})";
            }

            if (!string.IsNullOrEmpty(listInfo.MonthColumn))
            {
                excelRowValues[listInfo.MonthColumn[0] - FirstExcelColumn] = $"=MONTH({listInfo.DateColumn}{row})";
            }

            if (!string.IsNullOrEmpty(listInfo.DateColumn))
            {
                excelRowValues[listInfo.DateColumn[0] - FirstExcelColumn] = income.Date.ToString("dd.MM.yyyy");
            }

            if (!string.IsNullOrEmpty(listInfo.CategoryColumn))
            {
                excelRowValues[listInfo.CategoryColumn[0] - FirstExcelColumn] = income.Category;
            }

            if (!string.IsNullOrEmpty(listInfo.DescriptionColumn))
            {
                excelRowValues[listInfo.DescriptionColumn[0] - FirstExcelColumn] = income.Description;
            }

            if (!string.IsNullOrEmpty(listInfo.AmountRurColumn))
            {
                excelRowValues[listInfo.AmountRurColumn[0] - FirstExcelColumn] =
                    income.Currency == Currency.RUR ? income.Amount : "";
            }

            if (!string.IsNullOrEmpty(listInfo.AmountAmdColumn))
            {
                excelRowValues[listInfo.AmountAmdColumn[0] - FirstExcelColumn] =
                    income.Currency == Currency.AMD ? income.Amount : "";
            }
            
            if (!string.IsNullOrEmpty(listInfo.AmountGelColumn))
            {
                excelRowValues[listInfo.AmountGelColumn[0] - FirstExcelColumn] =
                    income.Currency == Currency.GEL ? income.Amount : "";
            }

            while (excelRowValues[^1] == null)
            {
                excelRowValues.RemoveAt(excelRowValues.Count - 1);
            }

            var result = new List<IList<object>>();
            result.Add(excelRowValues);

            return result;
        }
        
        private List<IList<object>> FillExcelRows(List<Expense> expenses, ListInfo listInfo, int firstRow)
        {
            var result = new List<IList<object>>(expenses.Count);

            for (int i = 0; i < expenses.Count; i++)
            {
                var excelRowValues = new List<object>();
                var expense = expenses[i];
                var row = firstRow + i;

                for (int j = 0; j < 10; j++)
                {
                    excelRowValues.Add(null);
                }

                if (!string.IsNullOrEmpty(listInfo.YearColumn))
                {
                    excelRowValues[listInfo.YearColumn[0] - FirstExcelColumn] = $"=YEAR({listInfo.DateColumn}{row})";
                }

                if (!string.IsNullOrEmpty(listInfo.MonthColumn))
                {
                    excelRowValues[listInfo.MonthColumn[0] - FirstExcelColumn] = $"=MONTH({listInfo.DateColumn}{row})";
                }

                if (!string.IsNullOrEmpty(listInfo.DateColumn))
                {
                    excelRowValues[listInfo.DateColumn[0] - FirstExcelColumn] = expense.Date.ToString("dd.MM.yyyy");
                }

                if (!string.IsNullOrEmpty(listInfo.CategoryColumn))
                {
                    excelRowValues[listInfo.CategoryColumn[0] - FirstExcelColumn] = expense.Category;
                }

                if (!string.IsNullOrEmpty(listInfo.SubCategoryColumn))
                {
                    excelRowValues[listInfo.SubCategoryColumn[0] - FirstExcelColumn] = expense.Subcategory;
                }

                if (!string.IsNullOrEmpty(listInfo.DescriptionColumn))
                {
                    excelRowValues[listInfo.DescriptionColumn[0] - FirstExcelColumn] = expense.Description;
                }

                if (!string.IsNullOrEmpty(listInfo.AmountRurColumn))
                {
                    excelRowValues[listInfo.AmountRurColumn[0] - FirstExcelColumn] =
                        expense.Currency == Currency.RUR ? expense.Amount : "";
                }

                if (!string.IsNullOrEmpty(listInfo.AmountAmdColumn))
                {
                    excelRowValues[listInfo.AmountAmdColumn[0] - FirstExcelColumn] =
                        expense.Currency == Currency.AMD ? expense.Amount : "";
                }
                
                if (!string.IsNullOrEmpty(listInfo.AmountGelColumn))
                {
                    excelRowValues[listInfo.AmountGelColumn[0] - FirstExcelColumn] =
                        expense.Currency == Currency.GEL ? expense.Amount : "";
                }

                while (excelRowValues[^1] == null)
                {
                    excelRowValues.RemoveAt(excelRowValues.Count - 1);
                }
                
                result.Add(excelRowValues);
            }

            return result;
        }

        private async Task<SheetsService> InitializeService(CancellationToken cancellationToken)
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            using (Stream stream = assembly.GetManifestResourceStream("GoogleSheetWriter.servicekey.json"))
            using (StreamReader reader = new StreamReader(stream))
            {
                string serviceKey = await reader.ReadToEndAsync();
                _credential = GoogleCredential.FromJson(serviceKey);
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = _applicationName,
            });
            return service;
        }


        private async Task<int> GetNumberFilledRows(SheetsService service, string listName,
            CancellationToken cancellationToken)
        {
            var request = service.Spreadsheets.Get(_spreadsheetId);
            request.IncludeGridData = true;
            request.Ranges = $"{listName}!A1:B";
            var response = await request.ExecuteAsync(cancellationToken);
            var sheet = response.Sheets.First(s => s.Properties.Title == listName);

            int i = 0;
            foreach (var data in sheet.Data)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var rowData in data.RowData)
                {
                    bool filled = false;
                    if (rowData.Values == null) break;
                    
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

        public async Task<List<Expense>> ReadExpenses(SearchOption searchOptions, CancellationToken cancellationToken)
        {
            var service = await InitializeService(cancellationToken);

            var result = new List<Expense>();
            foreach (var list in new []{_options.EveryDayExpenses, _options.FlatInfo, _options.BigDealInfo, _options.CurrencyConversion})
            {
                result.AddRange(
                    await GetRows(service, list, searchOptions, cancellationToken)
                    );
            }

            return result;
        }
        
        public async Task<List<Income>> ReadIncomes(SearchOption searchOptions, CancellationToken cancellationToken)
        {
            var service = await InitializeService(cancellationToken);

            var rows = await GetRows(service, _options.Incomes, searchOptions, cancellationToken);

            var incomes = new List<Income>(rows.Count);
            foreach (var expense in rows)
            {
                incomes.Add(new Income()
                {
                    Date = expense.Date,
                    Category = expense.Category,
                    Description = expense.Description,
                    Amount = expense.Amount,
                    Currency = expense.Currency
                });
            }

            return incomes;
        }

        private async Task<List<Expense>> GetRows(SheetsService service, ListInfo info, SearchOption searchOptions, CancellationToken cancellationToken)
        {
            List<Expense> expenses = new();

            var factory = SheetRowFactory.FromListInfo(info, _cultureInfo);

            int fromRangeRow = 1;
            if (searchOptions.DateFrom != null &&
                info.YearToFirstExcelRow.TryGetValue(searchOptions.DateFrom.Value.Year, out var rowNumber))
            {
                fromRangeRow = rowNumber;
            }
            
            int toRangeRow = fromRangeRow + BatchSize - 1;
            
            int lastFilledRow = await GetNumberFilledRows(service, info.ListName, cancellationToken);
            _logger.LogInformation($"List: {info.ListName} FromRange {fromRangeRow} Last Filled Row = {lastFilledRow}");

            while (fromRangeRow < lastFilledRow)
            {
                // logger.LogInformation(
                //     $"{info.ListName}. Range is {fromRangeRow}:{toRangeRow}. Last filled row is {lastFilledRow}");
                var request = service.Spreadsheets.Get(_spreadsheetId);
                request.IncludeGridData = true;
                request.Ranges = $"{info.ListName}!{info.DateColumn}{fromRangeRow}:{info.AmountGelColumn}{toRangeRow}";
                var response = await request.ExecuteAsync(cancellationToken);
                var sheet = response.Sheets.First(s => s.Properties.Title == info.ListName);

                foreach (var data in sheet.Data)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    foreach (var cellData in data.RowData)
                    {
                        if (cellData.Values == null) continue;
                        if (new[] {"Дата", "Год", "", null}.Contains(cellData.Values[0].FormattedValue)) continue;

                        var expense = factory.CreateExpense(cellData.Values);

                        if (searchOptions.IsSatisfied(expense))
                        {
                            expenses.Add(expense);
                        }
                    }
                }

                fromRangeRow += BatchSize;
                toRangeRow += BatchSize;
            }

            return expenses;
        }
    }
}