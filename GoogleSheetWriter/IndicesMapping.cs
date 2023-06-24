namespace GoogleSheetWriter
{
    internal struct IndicesMapping
    {
        internal int DateIndex;
        internal int? CategoryIndex;
        internal int? SubcategoryIndex;
        internal int? DescriptionIndex;
        internal int RurAmountIndex;
        internal int AmdAmountIndex;
        internal string? DefaultCategory;

        internal IndicesMapping(int date, int? category, int? subcategory, int? description, int rurAmount,
            int amdAmount, string? defaultCategory)
        {
            DateIndex = date;
            CategoryIndex = category;
            SubcategoryIndex = subcategory;
            DescriptionIndex = description;
            RurAmountIndex = rurAmount;
            AmdAmountIndex = amdAmount;
            DefaultCategory = defaultCategory;
        }
    }
}