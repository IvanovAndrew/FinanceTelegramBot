using Domain;

namespace Application.Test.Extensions;

public class CategoryBuilder(string name, bool isDefault = false)
{
    private readonly List<string> _subcategories = new();

    public CategoryBuilder WithSubcategory(string subcategory)
    {
        _subcategories.Add(subcategory);
        return this;
    }

    public Category Build()
    {
        return new Category()
            { Name = name, Subcategories = _subcategories.Select(c => new SubCategory{Name = c}).ToArray(), IsDefaultCategory = isDefault};
    }
}

public static class CategoryExtension
{
    internal static Category AsCategory(this string s)
    {
        return Category.FromString(s);
    }
}

public static class SubcategoryExtension
{
    internal static SubCategory? AsSubcategory(this string s)
    {
        return SubCategory.FromString(s);
    }
}