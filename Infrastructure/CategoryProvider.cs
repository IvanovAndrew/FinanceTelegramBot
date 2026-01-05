using Domain;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class CategoryProvider : ICategoryProvider
{
    private List<Category> OutcomeCategories { get; set; }
    private List<Category> IncomeCategories { get; set; }
    public CategoryProvider(IConfiguration configuration)
    {
        OutcomeCategories = configuration.GetSection("Categories").Get<List<Category>>();
        IncomeCategories = configuration
            .GetSection("IncomeCategories")
            .GetChildren()
            .Select(c => new Category
            {
                Name = c["Name"] ?? "",
                Type = Enum.TryParse<CategoryType>(c["Type"], out var t) ? t : CategoryType.OtherIncome,
            })
            .ToList();
    }

    public IReadOnlyList<Category> GetCategories(bool income)
    {
        return !income ? OutcomeCategories : IncomeCategories;
    }

    public Category DefaultOutcomeCategory()
    {
        return OutcomeCategories.First(c => c.IsDefaultCategory);
    }
}