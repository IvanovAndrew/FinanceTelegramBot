using System.Globalization;
using GoogleSheetWriter.Abstractions;
using GoogleSheetWriter.Domain;
using Microsoft.Extensions.Logging;

namespace GoogleSheetWriter.Infrastructure;

public class FutureExpenseRepository(
    IGoogleService googleService,
    SheetOptions options,
    ILogger<FutureExpenseRepository> logger)
    : IFutureExpenseRepository
{
    private readonly SheetRowReader _reader = new(googleService, logger);
    private readonly FutureExpenseListInfo _info = options.FutureExpenses;
    private readonly CultureInfo _culture = new("ru-RU");

    public async Task<IReadOnlyList<FutureExpense>> Read(string currency, CancellationToken cancellationToken)
    {
        var rows = await _reader.ReadRows(
            _info.ListName,
            _info.NameColumn,
            _info.IsActualColumn,
            null,
            null,
            cells => SheetRowFactory.CreateFutureExpense(_info, cells, _culture),
            (x) => x.Currency == currency,
            cancellationToken);

        return rows.Where(c => c.IsActual).ToList();
    }
}