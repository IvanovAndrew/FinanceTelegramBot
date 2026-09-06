using System.Globalization;
using GoogleSheetWriter.Domain;

namespace GoogleSheetWriter
{
    internal class SheetRowFactory(CultureInfo culture)
    {
        internal MoneyTransfer CreateMoneyTransfer(ListInfo info, IReadOnlyDictionary<ExcelColumn, CellData> cellData, bool isIncome)
        {
            var wrapper = new GoogleDataWrapper(cellData, info, culture);

            return new MoneyTransfer()
            {
                IsIncome = isIncome,
                Date = wrapper.Date,
                Category = wrapper.Category,
                Subcategory = wrapper.SubCategory,
                Shop = wrapper.Shop,
                Description = wrapper.Description,
                Amount = wrapper.Amount,
                Currency = wrapper.Currency
            };
        }
        
        // CurrencyExchangeListInfo carries explicit Source/Target amount+currency columns —
        // unlike MoneyTransfer there's no RUR/AMD/Other ambiguity to resolve, so this reads
        // columns directly instead of going through GoogleDataWrapper (which is typed to
        // ListInfo and doesn't know about CurrencyExchangeListInfo's shape).
        internal static CurrencyExchange CreateCurrencyExchange(
            CurrencyExchangeListInfo info,
            IReadOnlyDictionary<ExcelColumn, CellData> cellData,
            CultureInfo culture)
        {
            string? GetValue(ExcelColumn? column) =>
                column != null && cellData.TryGetValue(column, out var value) && value.Filled ? value.Value : null;

            decimal ParseAmount(string? raw) =>
                decimal.TryParse((raw ?? string.Empty).Trim(), NumberStyles.Currency, culture, out var value) ? value : 0;

            return new CurrencyExchange()
            {
                Date = DateOnly.TryParse(GetValue(info.DateColumn), culture, DateTimeStyles.None, out var date) ? date : DateOnly.MinValue,
                Shop = GetValue(info.ShopColumn),
                Description = GetValue(info.DescriptionColumn),
                SourceAmount = ParseAmount(GetValue(info.SourceAmountColumn)),
                SourceCurrency = GetValue(info.SourceCurrencyColumn) ?? string.Empty,
                TargetAmount = ParseAmount(GetValue(info.TargetAmountColumn)),
                TargetCurrency = GetValue(info.TargetCurrencyColumn) ?? string.Empty
            };
        }
        
        internal static FutureExpense CreateFutureExpense(
            FutureExpenseListInfo info,
            IReadOnlyDictionary<ExcelColumn, CellData> cellData,
            CultureInfo culture)
        {
            string? GetValue(ExcelColumn? column) =>
                column != null && cellData.TryGetValue(column, out var value) && value.Filled ? value.Value : null;

            decimal? ParseAmount(string? raw) =>
                decimal.TryParse((raw ?? string.Empty).Trim(), NumberStyles.Currency, culture, out var value) ? value : null;

            return new FutureExpense()
            {
                Name = GetValue(info.NameColumn),
                Category = GetValue(info.CategoryColumn),
                Subcategory = GetValue(info.SubCategoryColumn),
                Shop = GetValue(info.ShopColumn),
                
                Frequency = GetValue(info.FrequencyColumn),
                Way = GetValue(info.WayColumn),
                Amount = ParseAmount(GetValue(info.SumColumn)),
                Currency = GetValue(info.CurrencyColumn) ?? string.Empty
            };
        }
    }
}