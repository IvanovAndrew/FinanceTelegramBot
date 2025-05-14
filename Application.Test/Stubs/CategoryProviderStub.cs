using Application.Test.Extensions;
using Domain;
using UnitTest.Extensions;

namespace Application.Test.Stubs;

public class CategoryProviderStub : ICategoryProvider
{
    private readonly List<Category> _outcomeCategories =
    [
        new CategoryBuilder("Food", true).WithSubcategory("Snacks").WithSubcategory("Products").Build(),
        new CategoryBuilder("Cats").Build(),
        new CategoryBuilder("Online Services").Build(),
    ];

    private readonly List<Category> _incomeCategories = 
            [
                new CategoryBuilder("Other").Build()
            ];
    
    public IReadOnlyList<Category> GetCategories(bool income)
    {
        return income ? _incomeCategories : _outcomeCategories;
    }

    public Category DefaultOutcomeCategory()
    {
        return _outcomeCategories.First(c => c.IsDefaultCategory);
    }
}