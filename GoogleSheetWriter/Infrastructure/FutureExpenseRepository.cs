using System.Globalization;
using GoogleSheetWriter.Abstractions;
using GoogleSheetWriter.Domain;
using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter.Infrastructure;

public class FutureExpenseRepository : IFutureExpenseRepository
{
    private readonly IGoogleService _googleService;
    private readonly SheetRowReader _reader;
    private readonly FutureExpenseListInfo _info;
    private readonly CultureInfo _culture = new("ru-RU");

    public FutureExpenseRepository(IGoogleService googleService, SheetOptions options, ILogger<FutureExpenseRepository> logger)
    {
        _googleService = googleService;
        _reader = new SheetRowReader(googleService, logger);
        _info = options.FutureExpenses;
    }
    
    public async Task<IReadOnlyList<FutureExpense>> Read(string currency, CancellationToken cancellationToken)
    {
        var rows = await _reader.ReadRows(
            _info.ListName,
            _info.NameColumn,
            _info.CurrencyColumn,
            null,
            null,
            cells => SheetRowFactory.CreateFutureExpense(_info, cells, _culture),
            (x) => x.Currency == currency,
            cancellationToken);

        return rows;
    }
}