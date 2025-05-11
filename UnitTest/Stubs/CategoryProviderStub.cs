using Domain;
using UnitTest.Extensions;

namespace UnitTest.Stubs;

public class CategoryProviderStub : ICategoryProvider
{
    private readonly List<Category> _outcomeCategories =
    [
        new CategoryBuilder("Food").WithSubcategory("Snacks").WithSubcategory("Products").Build(),
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
}