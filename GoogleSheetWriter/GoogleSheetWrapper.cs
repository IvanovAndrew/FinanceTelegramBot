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
        private readonly IGoogleService _googleService;
        private readonly SheetOptions _options;
        private readonly CategoryToListMappingOptions _categoryMapping;
        private readonly ILogger<GoogleSheetWrapper> _logger;
        private readonly CultureInfo _cultureInfo = new("ru-RU");
        private const int BatchSize = 500;
        private char FirstExcelColumn = 'A';
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public GoogleSheetWrapper(IGoogleService googleService, SheetOptions options, CategoryToListMappingOptions mappingOptions, ILogger<GoogleSheetWrapper> logger)
        {
            _googleService = googleService;
            _options = options;
            _categoryMapping = mappingOptions;
            _logger = logger;
        }

        public async Task SaveAll(List<MoneyTransfer> expenses, CancellationToken cancellationToken)
        {
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
                int row = await GetNumberFilledRows(listInfo.ListName, cancellationToken) + 1;
                cancellationToken.ThrowIfCancellationRequested();

                // TODO Now there is inner rule that columns follow one by one. It can't be true in general and can lead to issues
                string range = $"{listInfo.ListName}!{listInfo.YearColumn}{row}:{listInfo.AmountGelColumn}{row + expenses.Count - 1}";

                var excelRowValues = FillExcelRows(expenses, listInfo, row);

                await _googleService.UpdateSheetAsync(range, excelRowValues, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveIncome(MoneyTransfer income, CancellationToken cancellationToken)
        {
            ListInfo listInfo = _options.Incomes;

            await _semaphore.WaitAsync(TimeSpan.FromMinutes(1), cancellationToken);
            try
            {
                int row = await GetNumberFilledRows(listInfo.ListName, cancellationToken) + 1;
                cancellationToken.ThrowIfCancellationRequested();

                // TODO Now there is inner rule that columns follow one by one. It can't be true in general and can lead to issues
                string range = $"{listInfo.ListName}!{listInfo.YearColumn}{row}:{listInfo.AmountGelColumn}{row}";

                var excelRowValues = FillExcelRows(income, listInfo, row);
                await _googleService.UpdateSheetAsync(range, excelRowValues, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private List<IList<object>> FillExcelRows(MoneyTransfer income, ListInfo listInfo, int firstRow)
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
        
        private List<IList<object>> FillExcelRows(List<MoneyTransfer> expenses, ListInfo listInfo, int firstRow)
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

        private async Task<int> GetNumberFilledRows(string listName, CancellationToken cancellationToken)
        {
            var sheet = await _googleService.GetSheetAsync(listName, new GoogleRequestOptions() { Range = $"{listName}!A1:B", RequestedColumns = new []{"A", "B"}}, cancellationToken);

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

        public async Task<List<MoneyTransfer>> ReadExpenses(MoneyTransferSearchOption searchOptions, CancellationToken cancellationToken)
        {
            var expenses = await 
            Task.WhenAll(
                new[]
                    {
                        _options.EveryDayExpenses, _options.FlatInfo, _options.BigDealInfo, _options.CurrencyConversion
                    }
                    .Select(list => GetRows(list, searchOptions, false, cancellationToken)));

            return expenses.SelectMany(e => e).ToList();
        }
        
        public async Task<List<MoneyTransfer>> ReadIncomes(MoneyTransferSearchOption searchOptions, CancellationToken cancellationToken)
        {
            var result = await Task.WhenAll(
                GetRows(_options.Incomes, searchOptions, true, cancellationToken), 
                GetRows(_options.CurrencyConversionIncome, searchOptions, true, cancellationToken)
                );
            
            return result.SelectMany(c => c).ToList();
        }

        private async Task<List<MoneyTransfer>> GetRows(ListInfo info, MoneyTransferSearchOption searchOptions, bool isIncome, CancellationToken cancellationToken)
        {
            List<MoneyTransfer> expenses = new();

            var factory = new SheetRowFactory(info, _cultureInfo);

            int fromRangeRow = 1;
            if (searchOptions.DateFrom != null &&
                info.YearToFirstExcelRow.TryGetValue(searchOptions.DateFrom.Value.Year, out var rowNumber))
            {
                fromRangeRow = rowNumber;
            }
            
            int toRangeRow = fromRangeRow + BatchSize - 1;
            
            int lastFilledRow = await GetNumberFilledRows(info.ListName, cancellationToken);
            _logger.LogInformation($"List: {info.ListName} FromRange {fromRangeRow} Last Filled Row = {lastFilledRow}");

            while (fromRangeRow < lastFilledRow)
            {
                var sheet = await _googleService.GetSheetAsync(info.ListName,
                    new GoogleRequestOptions()
                    {
                        Range = $"{info.ListName}!{info.DateColumn}{fromRangeRow}:{info.AmountGelColumn}{toRangeRow}",
                        RequestedColumns = GoogleRequestOptions.GetColumnsBetween(info.DateColumn, info.AmountGelColumn),
                    }, cancellationToken);

                foreach (var data in sheet.Data)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    foreach (var rowData in data.RowData)
                    {
                        if (!rowData.Cells.Any()) continue;
                        if (rowData.ContainsValue("Дата", "Год")) continue;

                        var expense = factory.CreateMoneyTransfer(rowData.Cells, isIncome);

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