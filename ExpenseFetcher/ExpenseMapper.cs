namespace ExpenseFetcher;

internal class ExpenseMapper
{
    private static Dictionary<string, int> _categoryDictionary;
    private static Dictionary<string, int> _subcategoryDictionary;
    private static Dictionary<string, int> _currencyDictionary;

    internal ExpenseMapper(Dictionary<string, int> categoryDictionary, Dictionary<string, int> subcategoryDictionary, Dictionary<string, int> currencyDictionary)
    {
        _categoryDictionary = categoryDictionary;
        _subcategoryDictionary = subcategoryDictionary;
        _currencyDictionary = currencyDictionary;
    }
    
    public DbExpense MapExpense(IExpense expense)
    {
        return new DbExpense()
        {
            Date = expense.Date,
            Category = MapCategory(expense.Category),
            Subcategory = MapSubcategory(expense.SubCategory),
            Description = expense.Description,
            Amount = expense.Amount.Amount,
            Currency = MapCurrency(expense.Amount.Currency)
        };
    }

    private int MapCategory(string category)
    {
        return _categoryDictionary.TryGetValue(category, out var id) ? id : -1;
    }

    private int? MapSubcategory(string? subCategory)
    {
        if (subCategory == null)
            return null;

        return _subcategoryDictionary.TryGetValue(subCategory, out var id) ? id : null;
    }
    
    private int MapCurrency(string currency)
    {
        return _currencyDictionary.TryGetValue(currency, out var id) ? id : 1;
    }

    public static async Task<ExpenseMapper> InitAsync(Repository repository)
    {
        var categories = await repository.ReadCategory();
        var subCategories = await repository.ReadSubcategory();
        var currencies = await repository.ReadCurrency();

        return new ExpenseMapper(
            categories.ToDictionary(category => category.Name, category => category.Id,
                StringComparer.InvariantCultureIgnoreCase),
            subCategories.ToDictionary(subcategory => subcategory.Name, subcategory => subcategory.Id,
                StringComparer.InvariantCultureIgnoreCase),
            currencies.ToDictionary(currency => currency.Name, currency => currency.Id,
                StringComparer.InvariantCultureIgnoreCase));
    }
}