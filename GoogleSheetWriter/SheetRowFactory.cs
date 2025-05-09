using System.Globalization;

namespace GoogleSheetWriter
{
    internal class SheetRowFactory(ListInfo info, CultureInfo culture)
    {
        internal MoneyTransfer CreateMoneyTransfer(IReadOnlyDictionary<ExcelColumn, CellData> cellData, bool isIncome)
        {
            var wrapper = new GoogleDataWrapper(cellData, info, culture);

            return new MoneyTransfer()
            {
                IsIncome = isIncome,
                Date = wrapper.Date.ToDateTime(default),
                Category = wrapper.Category,
                Subcategory = wrapper.SubCategory,
                Description = wrapper.Description,
                Amount = wrapper.Amount,
                Currency = wrapper.Currency
            };
        }
    }
}