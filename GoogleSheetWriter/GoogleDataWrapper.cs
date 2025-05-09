using System.Globalization;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GoogleSheetWriter.Test")]

namespace GoogleSheetWriter
{
    internal class GoogleDataWrapper
    {
        private readonly IReadOnlyDictionary<ExcelColumn, CellData> _cellData;
        private readonly ListInfo _columnNames;
        private readonly CultureInfo _culture;

        internal GoogleDataWrapper(IReadOnlyDictionary<ExcelColumn, CellData> cellData, ListInfo columnNames, CultureInfo culture)
        {
            _cellData = cellData;
            _culture = culture;
            _columnNames = columnNames;
        }

        public DateOnly Date => DateOnly.TryParse(GetByColumnName(_columnNames.DateColumn), _culture, DateTimeStyles.None, out var date)? date : DateOnly.MinValue;

        public string Category =>
            _columnNames.Category != null ? _columnNames.Category! :
             GetByColumnName(_columnNames.CategoryColumn)?? "UNKNOWN";

        public string? SubCategory => GetByColumnName(_columnNames.SubCategoryColumn);
        public string? Description => GetByColumnName(_columnNames.DescriptionColumn);

        public decimal Amount => ParseAmount(GetByColumnName(_columnNames.AmountRurColumn), GetByColumnName(_columnNames.AmountAmdColumn), GetByColumnName(_columnNames.AmountGelColumn), GetByColumnName(_columnNames.AmountUsdColumn), GetByColumnName(_columnNames.AmountEurColumn));

        public Currency Currency => ParseCurrency(GetByColumnName(_columnNames.AmountRurColumn),
            GetByColumnName(_columnNames.AmountAmdColumn), GetByColumnName(_columnNames.AmountGelColumn), GetByColumnName(_columnNames.AmountUsdColumn), GetByColumnName(_columnNames.AmountEurColumn)); 

        private string? GetByColumnName(ExcelColumn? excelColumn)
        {
            if (excelColumn == null || !_cellData.TryGetValue(excelColumn, out var value) || !value.Filled) return null;

            return value.Value;
        }

        private decimal ParseAmount(params string[] values)
        {
            foreach (var s in values)
            {
                var trimmedValue = Normalize(s);
                if (decimal.TryParse(trimmedValue, NumberStyles.Currency, _culture, out var value))
                {
                    return value;
                }
            }

            return 0;
        }

        private Currency ParseCurrency(
            string? rurColumn, string? amdColumn, string? gelColumn, string? usdColumn, string? eurColumn)
        {
            var currencies = new (string? Column, Currency Type)[]
            {
                (rurColumn, Currency.RUR),
                (amdColumn, Currency.AMD),
                (gelColumn, Currency.GEL),
                (usdColumn, Currency.USD),
                (eurColumn, Currency.EUR),
            };

            foreach (var (column, currency) in currencies)
            {
                var value = Normalize(column);
                if (currency == Currency.RUR && value.Contains("Загрузка", StringComparison.CurrentCultureIgnoreCase))
                    return Currency.RUR;

                if (decimal.TryParse(value, NumberStyles.Currency, _culture, out var parsed))
                    return currency;
            }

            return Currency.RUR;

            //throw new ArgumentOutOfRangeException($"Couldn't parse money from columns: {string.Join(", ", currencies.Select(c => c.Column))}");
        }
        
        private string Normalize(string? input) => (input ?? string.Empty).Trim();
    }
}