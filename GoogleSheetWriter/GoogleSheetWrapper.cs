using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter
{
    public class GoogleSheetWrapper
    {
        private SheetOptions _options;
        private CategoryToListMappingOptions _categoryMapping;
        private string _applicationName;
        private string _spreadsheetId;
        private GoogleCredential _credential;
        private CultureInfo _cultureInfo = new("ru-RU");
        private const int BatchSize = 500;
        private char FirstExcelColumn = 'A';

        public GoogleSheetWrapper(SheetOptions options, CategoryToListMappingOptions mappingOptions, string applicationName, string spreadsheetId)
        {
            _options = options;
            _categoryMapping = mappingOptions;
            _applicationName = applicationName;
            _spreadsheetId = spreadsheetId;
        }

        public async Task Write(IExpense expense, CancellationToken cancellationToken)
        {
            var service = await InitializeService(cancellationToken);

            if (!_categoryMapping.CategoryToList.TryGetValue(expense.Category, out string listName))
            {
                listName = _categoryMapping.DefaultCategory;
            }

            ListInfo listInfo = _options.UsualExpenses;
            if (listName == _options.FlatInfo.ListName)
            {
                listInfo = _options.FlatInfo;
            }
            else if (listName == _options.BigDealInfo.ListName)
            {
                listInfo = _options.BigDealInfo;
            }
            
            int row = await GetNumberFilledRows(service, listInfo.ListName, cancellationToken) + 1;
            cancellationToken.ThrowIfCancellationRequested();

            // Define request parameters.
            var money = expense.Amount;
            
            // TODO Now there is inner rule that columns follow one by one. It can't be true in general and can lead to issues
            string range = $"{listInfo.ListName}!{listInfo.YearColumn}{row}:{listInfo.AmountAmdColumn}{row}";

            var excelRowValues = FillExcelRows(expense, listInfo, row, money);

            var valueRange = new ValueRange
            {
                Range = range,
                MajorDimension = "ROWS",
                Values = new List<IList<object>>
                {
                    excelRowValues
                },
            };

            SpreadsheetsResource.ValuesResource.UpdateRequest request =
                service.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
            request.ValueInputOption =
                SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            await request.ExecuteAsync(cancellationToken);
        }

        private List<object> FillExcelRows(IExpense expense, ListInfo listInfo, int row, Money money)
        {
            var excelRowValues = new List<object>();
            for (int i = 0; i < 10; i++)
            {
                excelRowValues.Add(null);
            }

            if (listInfo.YearColumn != "")
            {
                excelRowValues[listInfo.YearColumn[0] - FirstExcelColumn] = $"=YEAR({listInfo.DateColumn}{row})";
            }

            if (listInfo.MonthColumn != "")
            {
                excelRowValues[listInfo.MonthColumn[0] - FirstExcelColumn] = $"=MONTH({listInfo.DateColumn}{row})";
            }

            if (listInfo.DateColumn != "")
            {
                excelRowValues[listInfo.DateColumn[0] - FirstExcelColumn] = expense.Date.ToString("dd.MM.yyyy");
            }

            if (!string.IsNullOrEmpty(listInfo.CategoryColumn))
            {
                excelRowValues[listInfo.CategoryColumn[0] - FirstExcelColumn] = expense.Category;
            }

            if (!string.IsNullOrEmpty(listInfo.SubCategoryColumn))
            {
                excelRowValues[listInfo.SubCategoryColumn[0] - FirstExcelColumn] = expense.SubCategory;
            }

            if (!string.IsNullOrEmpty(listInfo.DescriptionColumn))
            {
                excelRowValues[listInfo.DescriptionColumn[0] - FirstExcelColumn] = expense.Description;
            }

            if (!string.IsNullOrEmpty(listInfo.AmountRurColumn))
            {
                excelRowValues[listInfo.AmountRurColumn[0] - FirstExcelColumn] =
                    money.Currency == Currency.Rur ? money.Amount : "";
            }

            if (!string.IsNullOrEmpty(listInfo.AmountAmdColumn))
            {
                excelRowValues[listInfo.AmountAmdColumn[0] - FirstExcelColumn] =
                    money.Currency == Currency.Amd ? money.Amount : "";
            }

            while (excelRowValues[^1] == null)
            {
                excelRowValues.RemoveAt(excelRowValues.Count - 1);
            }

            return excelRowValues;
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


        private async Task<int> GetNumberFilledRows(SheetsService service, string listName,
            CancellationToken cancellationToken)
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

        public async Task<List<Expense>> GetRows(Predicate<DateOnly> dateFilter, ILogger logger,
            CancellationToken cancellationToken)
        {
            var service = await InitializeService(cancellationToken);

            var usualExpenses =
                await GetRows(service, _options.UsualExpenses, "", dateFilter, logger, cancellationToken);
            var flatExpenses = await GetRows(service, _options.FlatInfo, "Квартира", dateFilter, logger,
                cancellationToken);
            var bigExpenses = await GetRows(service, _options.BigDealInfo, "Крупные", dateFilter, logger,
                cancellationToken);

            return usualExpenses.Union(flatExpenses).Union(bigExpenses).OrderBy(e => e.Date).ToList();
        }

        private async Task<List<Expense>> GetRows(SheetsService service, ListInfo info, string category,
            Predicate<DateOnly> filter, ILogger logger, CancellationToken cancellationToken)
        {
            var dateIndex = 0;
            int? categoryIndex = string.IsNullOrEmpty(category) ? info.CategoryColumn[0] - info.DateColumn[0] : null;
            int? subCategoryIndex = !string.IsNullOrEmpty(info.SubCategoryColumn)
                ? info.SubCategoryColumn[0] - info.DateColumn[0]
                : null;
            int? descriptionIndex = !string.IsNullOrEmpty(info.DescriptionColumn)
                ? info.DescriptionColumn[0] - info.DateColumn[0]
                : null;
            int rurAmountIndex = info.AmountRurColumn[0] - info.DateColumn[0];
            int amdAmountIndex = info.AmountAmdColumn[0] - info.DateColumn[0];

            List<Expense> expenses = new();

            int fromRangeRow = 1;
            int toRangeRow = BatchSize;

            int lastFilledRow = await GetNumberFilledRows(service, info.ListName, cancellationToken);

            while (fromRangeRow < lastFilledRow)
            {
                logger.LogInformation($"{info.ListName}. Range is {fromRangeRow}:{toRangeRow}. Last filled row is {lastFilledRow}");
                var request = service.Spreadsheets.Get(_spreadsheetId);
                request.IncludeGridData = true;
                request.Ranges = $"{info.ListName}!{info.DateColumn}{fromRangeRow}:{info.AmountAmdColumn}{toRangeRow}";
                var response = await request.ExecuteAsync(cancellationToken);
                var sheet = response.Sheets.First(s => s.Properties.Title == info.ListName);

                int i = 0;
                foreach (var data in sheet.Data)
                {
                    foreach (var rowData in data.RowData)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (rowData.Values == null) continue;

                        bool filled = false;
                        if (DateTime.TryParse(rowData.Values[dateIndex].FormattedValue, _cultureInfo,
                            DateTimeStyles.None,
                            out var date) && filter(DateOnly.FromDateTime(date)))
                        {
                            expenses.Add(
                                new Expense
                                {
                                    Date = DateOnly.FromDateTime(date),
                                    Category = categoryIndex != null
                                        ? rowData.Values[categoryIndex.Value].FormattedValue
                                        : category,
                                    SubCategory = subCategoryIndex != null
                                        ? rowData.Values[subCategoryIndex.Value].FormattedValue
                                        : null,
                                    Description = descriptionIndex != null
                                        ? rowData.Values[descriptionIndex.Value].FormattedValue
                                        : null,
                                    Amount = ParseMoney(rowData.Values[rurAmountIndex].FormattedValue,
                                        rowData.Values.Count > amdAmountIndex
                                            ? rowData.Values[amdAmountIndex].FormattedValue
                                            : null),
                                });
                        }

                        i++;
                    }
                }


                fromRangeRow += BatchSize;
                toRangeRow += BatchSize;
            }

            return expenses;
        }

        private Money ParseMoney(string rurValue, string amdValue)
        {
            if (string.IsNullOrEmpty(rurValue) && string.IsNullOrEmpty(amdValue))
                return new Money() {Currency = Currency.Amd, Amount = 0m};

            if (Money.TryParse(rurValue, Currency.Rur, _cultureInfo, out var money))
            {
                return money;
            }

            else if (Money.TryParse(amdValue, Currency.Amd, _cultureInfo, out money))
            {
                return money;
            }

            throw new ArgumentOutOfRangeException($"Couldn't parse money from {rurValue} and {amdValue}");
        }
    }
}