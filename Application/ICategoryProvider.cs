using Domain;

public interface ICategoryProvider
{
    public IReadOnlyList<Category> GetCategories(bool income);
    public Category DefaultOutcomeCategory();
    public Category? GetCategoryByName(string categoryName, bool income)
    {
        return GetCategories(income)
            .FirstOrDefault(c => 
                string.Equals(c.ShortName, categoryName, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(c.Name, categoryName, StringComparison.InvariantCultureIgnoreCase));
    }
}