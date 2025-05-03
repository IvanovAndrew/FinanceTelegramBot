using Domain;

namespace Domain
{
    public class Category
    {
        public string Name { get; init; }
        public string? ShortName { get; init; }
        public SubCategory[] Subcategories { get; set; } = Array.Empty<SubCategory>();

        public Category()
        {
        
        }
    }

    public class SubCategory
    {
        public string Name { get; init; } = String.Empty;
        public string? ShortName { get; init; }
    }

    public class IncomeCategory
    {
        public string Name { get; init; }
    }
}

public interface ICategoryProvider
{
    public IReadOnlyList<Category> GetCategories(bool income);
}