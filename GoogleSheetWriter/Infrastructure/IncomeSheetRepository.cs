using System.Globalization;
using GoogleSheetWriter.Abstractions;
using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter.Infrastructure;

public class IncomeSheetRepository : IIncomeRepository
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IGoogleService _googleService;
    private readonly SheetRowReader _reader;
    private readonly ListInfo _info;
    private readonly CultureInfo _culture = new("ru-RU");

    public IncomeSheetRepository(IGoogleService googleService, SheetOptions options, ILogger<IncomeSheetRepository> logger)
    {
        _googleService = googleService;
        _reader = new SheetRowReader(googleService, logger);
        _info = options.Incomes;
    }

    public Task<IReadOnlyList<MoneyTransfer>> Read(MoneyTransferSearchOption searchOptions, CancellationToken cancellationToken)
    {
        var factory = new SheetRowFactory(_culture);
        return ReadInternal(searchOptions, factory, cancellationToken);
    }

    private async Task<IReadOnlyList<MoneyTransfer>> ReadInternal(MoneyTransferSearchOption searchOptions, SheetRowFactory factory, CancellationToken cancellationToken)
    {
        return await _reader.ReadRows(
            _info.ListName,
            _info.DateColumn,
            _info.GetLastExcelColumn(),
            _info.DateRowResolver,
            searchOptions.DateFrom,
            cells => factory.CreateMoneyTransfer(_info, cells, isIncome: true),
            searchOptions.IsSatisfied,
            cancellationToken);
    }

    public async Task Write(MoneyTransfer income, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(TimeSpan.FromMinutes(1), cancellationToken);
        try
        {
            int row = await _reader.GetNumberFilledRows(_info.ListName, cancellationToken) + 1;
            cancellationToken.ThrowIfCancellationRequested();

            string range = _reader.BuildRange(_info.ListName, _info.GetFirstExcelColumn(), _info.GetLastExcelColumn(), row, 1);

            var excelRowValues = FillExcelRow(income, row);
            await _googleService.UpdateSheetAsync(range, excelRowValues, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private List<IList<object>> FillExcelRow(MoneyTransfer income, int row)
    {
        var excelRowValues = new List<object>();
        for (int j = 0; j < 10; j++) excelRowValues.Add(null);

        var firstExcelColumn = _info.GetFirstExcelColumn();

        if (_info.YearColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.YearColumn, firstExcelColumn)] = $"=YEAR({_info.DateColumn}{row})";

        if (_info.MonthColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.MonthColumn, firstExcelColumn)] = $"=MONTH({_info.DateColumn}{row})";

        if (_info.DateColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.DateColumn, firstExcelColumn)] = income.Date.ToString("dd.MM.yyyy");

        if (_info.CategoryColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.CategoryColumn, firstExcelColumn)] = income.Category;

        if (_info.DescriptionColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.DescriptionColumn, firstExcelColumn)] = income.Description;

        if (_info.AmountRurColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.AmountRurColumn, firstExcelColumn)] =
                income.Currency == Constants.Currency.RUR ? income.Amount : "";

        if (_info.AmountAmdColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.AmountAmdColumn, firstExcelColumn)] =
                income.Currency == Constants.Currency.AMD ? income.Amount : "";

        if (_info.OtherAmountColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.OtherAmountColumn, firstExcelColumn)] =
                !Constants.BaseCurrencies.Contains(income.Currency) ? income.Amount : "";

        if (_info.OtherCurrencyColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.OtherCurrencyColumn, firstExcelColumn)] =
                !Constants.BaseCurrencies.Contains(income.Currency) ? income.Currency.ToString() : "";

        while (excelRowValues[^1] == null)
        {
            excelRowValues.RemoveAt(excelRowValues.Count - 1);
        }

        return new List<IList<object>> { excelRowValues };
    }
}