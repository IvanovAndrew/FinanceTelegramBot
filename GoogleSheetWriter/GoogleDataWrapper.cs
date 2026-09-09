using System.Globalization;
using System.Runtime.CompilerServices;
using GoogleSheetWriter.Infrastructure;

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
            _columnNames.Category?? GetByColumnName(_columnNames.CategoryColumn)?? "UNKNOWN";

        public string? SubCategory => GetByColumnName(_columnNames.SubCategoryColumn);
        public string? Shop => GetByColumnName(_columnNames.ShopColumn);
        public string? Description => GetByColumnName(_columnNames.DescriptionColumn);

        public decimal Amount => ParseAmount(GetByColumnName(_columnNames.AmountRurColumn), GetByColumnName(_columnNames.AmountAmdColumn), GetByColumnName(_columnNames.OtherAmountColumn));

        public string Currency => ParseCurrency(GetByColumnName(_columnNames.AmountRurColumn),
            GetByColumnName(_columnNames.AmountAmdColumn), GetByColumnName(_columnNames.OtherAmountColumn), GetByColumnName(_columnNames.OtherCurrencyColumn)); 

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

        private string ParseCurrency(
            string? rurColumn, string? amdColumn, string? otherAmountColumn, string? otherCurrencyColumn)
        {
            var defaultCurrency = Constants.Currency.RUR; 
            
            var currencies = new (string? Column, string Type)[]
            {
                (rurColumn, Constants.Currency.RUR),
                (amdColumn, Constants.Currency.AMD),
            };

            foreach (var (column, currency) in currencies)
            {
                var value = Normalize(column);
                if (currency == nameof(Constants.Currency.RUR) && value.Contains("Загрузка", StringComparison.CurrentCultureIgnoreCase))
                    return defaultCurrency;

                if (decimal.TryParse(value, NumberStyles.Currency, _culture, out var parsed))
                    return currency;
            }

            if (decimal.TryParse(Normalize(otherAmountColumn), NumberStyles.Currency, _culture, out var parsedAmount) &&
                !string.IsNullOrEmpty(otherCurrencyColumn))
            {
                return otherCurrencyColumn;
            }

            return defaultCurrency;
        }
        
        private string Normalize(string? input) => (input ?? string.Empty).Trim();
    }
}