using System.Globalization;
using Google.Apis.Sheets.v4.Data;

namespace GoogleSheetWriter
{
    internal class SheetRowFactory
    {
        private readonly IndicesMapping _indices;
        private readonly CultureInfo _culture;

        private SheetRowFactory(IndicesMapping indicesMapping, CultureInfo culture)
        {
            _indices = indicesMapping;
            _culture = culture;
        }

        internal static SheetRowFactory FromListInfo(ListInfo listInfo, CultureInfo culture)
        {
            var dateIndex = 0;
            int? categoryIndex = null;
            int? subCategoryIndex = null;

            if (string.IsNullOrEmpty(listInfo.Category))
            {
                categoryIndex = listInfo.CategoryColumn[0] - listInfo.DateColumn[0];
            }

            if (!string.IsNullOrEmpty(listInfo.SubCategoryColumn))
            {
                subCategoryIndex = listInfo.SubCategoryColumn[0] - listInfo.DateColumn[0];
            }

            int? descriptionIndex = !string.IsNullOrEmpty(listInfo.DescriptionColumn)
                ? listInfo.DescriptionColumn[0] - listInfo.DateColumn[0]
                : null;
            int rurAmountIndex = listInfo.AmountRurColumn[0] - listInfo.DateColumn[0];
            int amdAmountIndex = listInfo.AmountAmdColumn[0] - listInfo.DateColumn[0];
            int gelAmountIndex = listInfo.AmountGelColumn[0] - listInfo.DateColumn[0];

            var indices = new IndicesMapping(dateIndex, categoryIndex, subCategoryIndex, descriptionIndex,
                rurAmountIndex, amdAmountIndex, gelAmountIndex, listInfo.Category);

            return new SheetRowFactory(indices, culture);
        }

        internal Expense CreateExpense(IList<CellData> cellData)
        {
            var wrapper = new GoogleDataWrapper(cellData, _indices, _culture);

            return new Expense()
            {
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