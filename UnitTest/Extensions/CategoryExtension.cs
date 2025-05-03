using Domain;

namespace UnitTest.Extensions;

public class CategoryBuilder
{
    private readonly string _name;
    private readonly List<string> _subcategories = new();

    public CategoryBuilder(string name)
    {
        _name = name;
    }

    public CategoryBuilder WithSubcategory(string subcategory)
    {
        _subcategories.Add(subcategory);
        return this;
    }

    public Category Build()
    {
        return new Category()
            { Name = _name, Subcategories = _subcategories.Select(c => new SubCategory{Name = c}).ToArray() };
    }
}