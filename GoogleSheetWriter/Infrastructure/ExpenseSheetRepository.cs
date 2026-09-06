using System.Globalization;
using GoogleSheetWriter.Abstractions;
using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter.Infrastructure;

public class ExpenseSheetRepository : IExpenseRepository
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IGoogleService _googleService;
    private readonly SheetRowReader _reader;
    private readonly SheetOptions _options;
    private readonly CategoryToListMappingOptions _categoryMapping;
    private readonly CultureInfo _culture = new("ru-RU");

    public ExpenseSheetRepository(
        IGoogleService googleService,
        SheetOptions options,
        CategoryToListMappingOptions categoryMapping,
        ILogger<ExpenseSheetRepository> logger)
    {
        _googleService = googleService;
        _reader = new SheetRowReader(googleService, logger);
        _options = options;
        _categoryMapping = categoryMapping;
    }

    public async Task<IReadOnlyList<MoneyTransfer>> Read(MoneyTransferSearchOption searchOptions, CancellationToken cancellationToken)
    {
        var lists = new[] { _options.EveryDayExpenses, _options.FlatInfo, _options.BigDealInfo };
        var factory = new SheetRowFactory(_culture);

        var results = await Task.WhenAll(lists.Select(info =>
            _reader.ReadRows(
                info.ListName,
                info.DateColumn,
                info.GetLastExcelColumn(),
                info.DateRowResolver,
                searchOptions.DateFrom,
                cells => factory.CreateMoneyTransfer(info, cells, isIncome: false),
                searchOptions.IsSatisfied,
                cancellationToken)));

        return results.SelectMany(r => r).ToList();
    }

    public async Task Write(IReadOnlyList<MoneyTransfer> expenses, CancellationToken cancellationToken)
    {
        if (expenses.Count == 0) return;

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
            // Напоминание из прошлого раза: CurrencyConversion — CurrencyExchangeListInfo, а не
            // ListInfo, писать MoneyTransfer сюда некуда. Если это условие сработало — значит,
            // вызывающий код должен был использовать ISheetRepository<CurrencyExchange>.
            throw new InvalidOperationException(
                "Currency exchange entries must be saved via ISheetRepository<CurrencyExchange>, not IExpenseRepository.");
        }

        await _semaphore.WaitAsync(TimeSpan.FromMinutes(1), cancellationToken);
        try
        {
            int row = await _reader.GetNumberFilledRows(listInfo.ListName, cancellationToken) + 1;
            cancellationToken.ThrowIfCancellationRequested();

            string range = _reader.BuildRange(listInfo.ListName, listInfo.GetFirstExcelColumn(), listInfo.GetLastExcelColumn(), row, expenses.Count);

            var excelRowValues = FillExcelRows(expenses, listInfo, row);

            await _googleService.UpdateSheetAsync(range, excelRowValues, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private List<IList<object>> FillExcelRows(IReadOnlyList<MoneyTransfer> expenses, ListInfo listInfo, int firstRow)
    {
        var firstExcelColumn = listInfo.GetFirstExcelColumn();
        var lastExcelColumn = listInfo.GetLastExcelColumn();
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
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.YearColumn, firstExcelColumn)] = $"=YEAR({listInfo.DateColumn}{row})";

            if (listInfo.MonthColumn is not null)
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.MonthColumn, firstExcelColumn)] = $"=MONTH({listInfo.DateColumn}{row})";

            if (listInfo.DateColumn is not null)
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.DateColumn, firstExcelColumn)] = expense.Date.ToString("dd.MM.yyyy");

            if (listInfo.CategoryColumn is not null)
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.CategoryColumn, firstExcelColumn)] = expense.Category;

            if (listInfo.SubCategoryColumn is not null)
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.SubCategoryColumn, firstExcelColumn)] = expense.Subcategory;

            if (listInfo.ShopColumn is not null)
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.ShopColumn, firstExcelColumn)] = expense.Shop;

            if (listInfo.DescriptionColumn is not null)
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.DescriptionColumn, firstExcelColumn)] = expense.Description;

            if (listInfo.AmountRurColumn is not null)
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountRurColumn, firstExcelColumn)] =
                    expense.Currency == Constants.Currency.RUR ? expense.Amount : "";

            if (listInfo.AmountAmdColumn is not null)
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.AmountAmdColumn, firstExcelColumn)] =
                    expense.Currency == Constants.Currency.AMD ? expense.Amount : "";

            if (listInfo.OtherAmountColumn is not null)
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.OtherAmountColumn, firstExcelColumn)] =
                    (expense.Currency != Constants.Currency.RUR && expense.Currency != Constants.Currency.AMD) ? expense.Amount : "";

            if (listInfo.OtherCurrencyColumn is not null)
                excelRowValues[ExcelColumn.DifferenceBetween(listInfo.OtherCurrencyColumn, firstExcelColumn)] =
                    (expense.Currency != Constants.Currency.RUR && expense.Currency != Constants.Currency.AMD) ? expense.Currency.ToString() : "";

            while (excelRowValues[^1] == null)
            {
                excelRowValues.RemoveAt(excelRowValues.Count - 1);
            }

            result.Add(excelRowValues);
        }

        return result;
    }
}