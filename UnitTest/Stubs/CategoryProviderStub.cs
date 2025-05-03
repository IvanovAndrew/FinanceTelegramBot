using Domain;

namespace UnitTest.Stubs;

public class CategoryProviderStub : ICategoryProvider
{
    private readonly Category[] _incomeCategories;
    private readonly Category[] _outcomeCategories;

    public IReadOnlyList<Category> GetCategories(bool income)
    {
        return income ? _incomeCategories : _outcomeCategories;
    }

    public CategoryProviderStub(Category[] outcomeCategories, Category[] incomeCategories)
    {
        _outcomeCategories = outcomeCategories;
        _incomeCategories = incomeCategories;
    }
}