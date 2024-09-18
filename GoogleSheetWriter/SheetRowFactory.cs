using System.Globalization;

namespace GoogleSheetWriter
{
    internal class SheetRowFactory
    {
        private readonly CultureInfo _culture;
        private readonly ListInfo _info;

        public SheetRowFactory(ListInfo info, CultureInfo culture)
        {
            _culture = culture;
            _info = info;
        }

        internal MoneyTransfer CreateMoneyTransfer(IReadOnlyDictionary<string, ICellData> cellData, bool isIncome)
        {
            var wrapper = new GoogleDataWrapper(cellData, _info, _culture);

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