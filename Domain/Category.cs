namespace Domain;

public class Category
{
    public string Name { get; set; }
    public SubCategory[] SubCategories { get; set; }

    public Category()
    {
        
    }
}

public class SubCategory
{
    public string Name { get; set; }
}