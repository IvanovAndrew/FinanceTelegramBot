using System.Globalization;
using Google.Apis.Sheets.v4.Data;

namespace GoogleSheetWriter
{
    internal class GoogleDataWrapper
    {
        private readonly IReadOnlyDictionary<string, ICellData> _cellData;
        private readonly ListInfo _columnNames;
        private readonly CultureInfo _culture;

        internal GoogleDataWrapper(IReadOnlyDictionary<string, ICellData> cellData, ListInfo columnNames, CultureInfo culture)
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

        public decimal Amount => ParseAmount(GetByColumnName(_columnNames.AmountRurColumn), GetByColumnName(_columnNames.AmountAmdColumn), GetByColumnName(_columnNames.AmountGelColumn));

        public Currency Currency => ParseCurrency(GetByColumnName(_columnNames.AmountRurColumn),
            GetByColumnName(_columnNames.AmountAmdColumn), GetByColumnName(_columnNames.AmountGelColumn)); 

        private string? GetByColumnName(string columnName)
        {
            if (string.IsNullOrEmpty(columnName) || !_cellData.TryGetValue(columnName, out var value) || !value.Filled) return null;

            return value.Value;
        }

        private decimal ParseAmount(params string[] values)
        {
            foreach (var s in values)
            {
                var trimmedValue = (s?? "").Trim();
                if (decimal.TryParse(trimmedValue, NumberStyles.Currency, _culture, out var value) && value != 0)
                {
                    return value;
                }
            }

            return 0;
        }
        
        private decimal Parse(params string[] values)
        {
            foreach (var s in values)
            {
                var trimmedValue = (s?? "").Trim();
                if (decimal.TryParse(trimmedValue, NumberStyles.Currency, _culture, out var value))
                {
                    return value;
                }
            }

            return -1;
        }

        private Currency ParseCurrency(string? rurValue, string? amdValue, string? gelValue)
        {
            string rur = (rurValue ?? String.Empty).Trim();
            string amd = (amdValue ?? String.Empty).Trim();
            string gel = (gelValue ?? String.Empty).Trim();
            if (string.IsNullOrEmpty(rur) && string.IsNullOrEmpty(amd) && string.IsNullOrEmpty(gel))
                return Currency.RUR;

            if (rur.Contains("Загрузка", StringComparison.CurrentCultureIgnoreCase))
            {
                return Currency.RUR;
            }

            if (Parse(rur) != -1)
            {
                return Currency.RUR;
            }

            else if (Parse(amd) != -1)
            {
                return Currency.AMD;
            }
            
            else if (Parse(gel) != -1)
            {
                return Currency.GEL;
            }

            throw new ArgumentOutOfRangeException($"Couldn't parse money from {rurValue}, {amdValue}, and {gelValue}");
        }
    }
}