using System.Globalization;
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
                string range = BuildRange(listInfo, listInfo.GetFirstExcelColumn(), row, expenses.Count);

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
                string range = BuildRange(listInfo, listInfo.GetFirstExcelColumn(), row, 1);

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

            var firstExcelColumn = listInfo.GetFirstExcelColumn();

            if (listInfo.YearColumn is not null)
            {
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.YearColumn, firstExcelColumn)] = $"=YEAR({listInfo.DateColumn}{row})";
            }

            if (listInfo.MonthColumn is not null)
            {
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.MonthColumn, firstExcelColumn)] = $"=MONTH({listInfo.DateColumn}{row})";
            }

            if (listInfo.DateColumn is not null)
            {
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.DateColumn, firstExcelColumn)] = income.Date.ToString("dd.MM.yyyy");
            }

            if (listInfo.CategoryColumn is not null)
            {
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.CategoryColumn, firstExcelColumn)] = income.Category;
            }

            if (listInfo.DescriptionColumn is not null)
            {
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.DescriptionColumn, firstExcelColumn)] = income.Description;
            }

            if (listInfo.AmountRurColumn is not null)
            {
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountRurColumn, firstExcelColumn)] =
                    income.Currency == Currency.RUR ? income.Amount : "";
            }

            if (listInfo.AmountAmdColumn is not null)
            {
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountAmdColumn, firstExcelColumn)] =
                    income.Currency == Currency.AMD ? income.Amount : "";
            }
            
            if (listInfo.AmountGelColumn is not null)
            {
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountGelColumn, firstExcelColumn)] =
                    income.Currency == Currency.GEL ? income.Amount : "";
            }
                
            if (listInfo.AmountUsdColumn is not null)
            {
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountUsdColumn, firstExcelColumn)] =
                    income.Currency == Currency.USD ? income.Amount : "";
            }
            
            if (listInfo.AmountEurColumn is not null)
            {
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountEurColumn, firstExcelColumn)] =
                    income.Currency == Currency.EUR ? income.Amount : "";
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
            var firstExcelColumn = listInfo.GetFirstExcelColumn();
            var lastExcelColumn = listInfo.GetLastExcelColumn();
            _logger.LogInformation($"list is {listInfo.ListName}. First column is {firstExcelColumn} and last column is {lastExcelColumn}");
            var count = ExcelColumn.DifferenceBetween(lastExcelColumn, firstExcelColumn) + 1;
            
            var result = new List<IList<object>>(expenses.Count);

            for (int i = 0; i < expenses.Count; i++)
            {
                var excelRowValues = new List<object>();
                var expense = expenses[i];
                var row = firstRow + i;

                for (int j = 0; j < count; j++)
                {
                    excelRowValues.Add(null);
                }

                if (listInfo.YearColumn is not null)
                {
                    excelRowValues[ExcelColumn.DifferenceBetween(listInfo.YearColumn, firstExcelColumn)] = $"=YEAR({listInfo.DateColumn}{row})";
                }

                if (listInfo.MonthColumn is not null)
                {
                    excelRowValues[ExcelColumn.DifferenceBetween(listInfo.MonthColumn, firstExcelColumn)] = $"=MONTH({listInfo.DateColumn}{row})";
                }

                if (listInfo.DateColumn is not null)
                {
                    excelRowValues[ExcelColumn.DifferenceBetween(listInfo.DateColumn, firstExcelColumn)] = expense.Date.ToString("dd.MM.yyyy");
                }

                if (listInfo.CategoryColumn is not null)
                {
                    excelRowValues[ExcelColumn.DifferenceBetween(listInfo.CategoryColumn, firstExcelColumn)] = expense.Category;
                }

                if (listInfo.SubCategoryColumn is not null)
                {
                    excelRowValues[ExcelColumn.DifferenceBetween(listInfo.SubCategoryColumn, firstExcelColumn)] = expense.Subcategory;
                }

                if (listInfo.DescriptionColumn is not null)
                {
                    excelRowValues[ExcelColumn.DifferenceBetween(listInfo.DescriptionColumn, firstExcelColumn)] = expense.Description;
                }

                if (listInfo.AmountRurColumn is not null)
                {
                    excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountRurColumn, firstExcelColumn)] =
                        expense.Currency == Currency.RUR ? expense.Amount : "";
                }

                if (listInfo.AmountAmdColumn is not null)
                {
                    excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountAmdColumn, firstExcelColumn)] =
                        expense.Currency == Currency.AMD ? expense.Amount : "";
                }
                
                if (listInfo.AmountGelColumn is not null)
                {
                    excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountGelColumn, firstExcelColumn)] =
                        expense.Currency == Currency.GEL ? expense.Amount : "";
                }
                
                if (listInfo.AmountUsdColumn is not null)
                {
                    excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountUsdColumn, firstExcelColumn)] =
                        expense.Currency == Currency.USD ? expense.Amount : "";
                }
                
                if (listInfo.AmountEurColumn is not null)
                {
                    excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountEurColumn, firstExcelColumn)] =
                        expense.Currency == Currency.EUR ? expense.Amount : "";
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
            var sheet = await _googleService.GetSheetAsync(listName, 
                new GoogleRequestOptions()
                {
                    Range = $"{listName}!A1:B", 
                    RequestedColumns = new []{"A", "B"}.Select(ExcelColumn.FromString).ToArray()
                }, cancellationToken);

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
            
            int lastFilledRow = await GetNumberFilledRows(info.ListName, cancellationToken);
            

            ExcelColumn[] requestedColumns = ExcelColumn.ColumnsBetween(info.DateColumn, info.GetLastExcelColumn());
            _logger.LogInformation($"List: {info.ListName} FromRange {fromRangeRow} Last Filled Row = {lastFilledRow}. ");

            while (fromRangeRow < lastFilledRow)
            {
                var sheet = await _googleService.GetSheetAsync(info.ListName,
                    new GoogleRequestOptions()
                    {
                        Range = BuildRange(info, info.DateColumn, fromRangeRow, BatchSize),
                        RequestedColumns = requestedColumns,
                    }, cancellationToken);

                foreach (var data in sheet.Data)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    foreach (var rowData in data.RowData)
                    {
                        if (!rowData.Cells.Any()) continue;
                        if (rowData.ContainsValue("Дата", "Год")) continue;

                        var cells = rowData.Cells.ToDictionary(k => ExcelColumn.FromString(k.Key), kvp => kvp.Value);

                        var expense = factory.CreateMoneyTransfer(cells, isIncome);

                        if (searchOptions.IsSatisfied(expense))
                        {
                            expenses.Add(expense);
                        }
                    }
                }

                fromRangeRow += BatchSize;
            }

            return expenses;
        }
        
        private string BuildRange(ListInfo listInfo, ExcelColumn firstColumn, int startRow, int rowCount)
        {
            var lastColumn = listInfo.GetLastExcelColumn();
            return $"{listInfo.ListName}!{firstColumn.Name}{startRow}:{lastColumn}{startRow + rowCount - 1}";
        }
    }
}