using System.Globalization;
using Google.Apis.Sheets.v4.Data;

namespace GoogleSheetWriter
{
    internal class GoogleDataWrapper
    {
        private readonly string[] _cellData;
        private readonly IndicesMapping _indices;
        private readonly CultureInfo _culture;

        internal GoogleDataWrapper(IList<CellData> cellData, IndicesMapping indices, CultureInfo culture)
        {
            _cellData = cellData.Select(c => c.FormattedValue).ToArray();
            _indices = indices;
            _culture = culture;
        }

        public DateOnly Date => DateOnly.Parse(_cellData[_indices.DateIndex], _culture);

        public string Category =>
            _indices.DefaultCategory != null ? _indices.DefaultCategory! :
            _indices.CategoryIndex != null ? _cellData[_indices.CategoryIndex.Value] : "UNKNOWN";

        public string? SubCategory => GetByIndex(_indices.SubcategoryIndex);
        public string? Description => GetByIndex(_indices.DescriptionIndex);

        public decimal Amount => ParseAmount(GetByIndex(_indices.RurAmountIndex), GetByIndex(_indices.AmdAmountIndex), GetByIndex(_indices.GelAmountIndex));

        public Currency Currency => ParseCurrency(GetByIndex(_indices.RurAmountIndex),
            GetByIndex(_indices.AmdAmountIndex), GetByIndex(_indices.GelAmountIndex)); 

        private string? GetByIndex(int? index)
        {
            if (index == null || _cellData.Length <= index.Value) return null;

            return _cellData[index.Value];
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