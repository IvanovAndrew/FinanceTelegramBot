namespace Domain
{
    public class Category
    {
        public string Name { get; init; }
        public SubCategory[] SubCategories { get; set; } = System.Array.Empty<SubCategory>();

        public Category()
        {
        
        }
    }

    public class SubCategory
    {
        public string Name { get; init; }
    }
}