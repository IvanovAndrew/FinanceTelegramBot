using System.Globalization;
using GoogleSheetWriter.Abstractions;
using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter.Infrastructure;

public class CurrencyExchangeFromSheetRepository : ISheetRepository<CurrencyExchange>
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IGoogleService _googleService;
    private readonly SheetRowReader _reader;
    private readonly CurrencyExchangeListInfo _info;
    private readonly CultureInfo _culture = new("ru-RU");

    public CurrencyExchangeFromSheetRepository(
        IGoogleService googleService,
        SheetOptions options,
        ILogger<CurrencyExchangeFromSheetRepository> logger)
    {
        _googleService = googleService;
        _reader = new SheetRowReader(googleService, logger);
        _info = options.CurrencyConversion;
    }

    public Task<IReadOnlyList<CurrencyExchange>> Read(MoneyTransferSearchOption searchOptions, CancellationToken cancellationToken)
    {
        return ReadInternal(searchOptions, cancellationToken);
    }

    private async Task<IReadOnlyList<CurrencyExchange>> ReadInternal(MoneyTransferSearchOption searchOptions, CancellationToken cancellationToken)
    {
        var rows = await _reader.ReadRows(
            _info.ListName,
            _info.DateColumn,
            _info.GetLastExcelColumn(),
            _info.DateRowResolver,
            searchOptions.DateFrom,
            cells => SheetRowFactory.CreateCurrencyExchange(_info, cells, _culture),
            exchange =>
                (searchOptions.DateFrom == null || searchOptions.DateFrom.Value <= exchange.Date) &&
                (searchOptions.DateTo == null || exchange.Date <= searchOptions.DateTo.Value),
            cancellationToken);

        return rows;
    }

    public async Task Write(IReadOnlyList<CurrencyExchange> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0) return;

        await _semaphore.WaitAsync(TimeSpan.FromMinutes(1), cancellationToken);
        try
        {
            int row = await _reader.GetNumberFilledRows(_info.ListName, cancellationToken) + 1;
            cancellationToken.ThrowIfCancellationRequested();

            var firstColumn = _info.GetFirstExcelColumn();
            var lastColumn = _info.GetLastExcelColumn();
            string range = _reader.BuildRange(_info.ListName, firstColumn, lastColumn, row, items.Count);

            var excelRowValues = items.Select((exchange, i) => FillExcelRow(exchange, row + i)).ToList();

            await _googleService.UpdateSheetAsync(range, excelRowValues, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private IList<object> FillExcelRow(CurrencyExchange exchange, int row)
    {
        var firstExcelColumn = _info.GetFirstExcelColumn();
        var lastExcelColumn = _info.GetLastExcelColumn();
        var count = ExcelColumn.DifferenceBetween(lastExcelColumn, firstExcelColumn) + 1;

        var excelRowValues = new List<object>();
        for (int j = 0; j < count; j++) excelRowValues.Add(null);

        if (_info.YearColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.YearColumn, firstExcelColumn)] = $"=YEAR({_info.DateColumn}{row})";

        excelRowValues[ExcelColumn.DifferenceBetween(_info.DateColumn, firstExcelColumn)] = exchange.Date.ToString("dd.MM.yyyy");

        if (_info.ShopColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.ShopColumn, firstExcelColumn)] = exchange.Shop;

        if (_info.DescriptionColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.DescriptionColumn, firstExcelColumn)] = exchange.Description;

        excelRowValues[ExcelColumn.DifferenceBetween(_info.SourceAmountColumn, firstExcelColumn)] = exchange.SourceAmount;
        excelRowValues[ExcelColumn.DifferenceBetween(_info.SourceCurrencyColumn, firstExcelColumn)] = exchange.SourceCurrency;
        excelRowValues[ExcelColumn.DifferenceBetween(_info.TargetAmountColumn, firstExcelColumn)] = exchange.TargetAmount;

        if (_info.TargetCurrencyColumn is not null)
            excelRowValues[ExcelColumn.DifferenceBetween(_info.TargetCurrencyColumn, firstExcelColumn)] = exchange.TargetCurrency;

        while (excelRowValues[^1] == null)
            excelRowValues.RemoveAt(excelRowValues.Count - 1);

        return excelRowValues;
    }
}