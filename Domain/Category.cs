namespace Domain
{
    public class Category
    {
        public string Name { get; init; }
        public string? ShortName { get; init; }
        public SubCategory[] SubCategories { get; set; } = Array.Empty<SubCategory>();

        public Category()
        {
        
        }
    }

    public class SubCategory
    {
        public string Name { get; init; } = String.Empty;
        public string? ShortName { get; init; }
    }
}