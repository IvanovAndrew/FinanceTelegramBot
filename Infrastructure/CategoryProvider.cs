using Domain;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class CategoryProvider : ICategoryProvider
{
    private List<Category> Categories { get; set; }
    public CategoryProvider(IConfiguration configuration)
    {
        Categories = configuration.GetSection("Categories").Get<List<Category>>();
    }

    public IReadOnlyList<Category> GetCategories(bool income)
    {
        if (!income)
        {
            return Categories;
        }
        
        return ArraySegment<Category>.Empty;
    }

    public Category DefaultOutcomeCategory()
    {
        return Categories.First(c => c.IsDefaultCategory);
    }
}