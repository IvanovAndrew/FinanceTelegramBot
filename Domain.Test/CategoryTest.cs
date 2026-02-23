namespace Domain.Test;

using Domain;
using Xunit;

public class CategoryEqualityTests
{
    [Fact]
    public void NullCategory_Equality()
    {
        Category? cat1 = null;
        Category? cat2 = null;

        Assert.True(cat1 == cat2);
        Assert.False(cat1 != cat2);
    }

    [Fact]
    public void NullAndNonNullCategory_Inequality()
    {
        Category? cat1 = null;
        Category? cat2 = Categories.Outcome.Pets;

        Assert.False(cat1 == cat2);
        Assert.True(cat1 != cat2);
    }

    [Fact]
    public void NullSubCategory_Equality()
    {
        SubCategory? sub1 = null;
        SubCategory? sub2 = null;

        Assert.True(sub1 == sub2);
        Assert.False(sub1 != sub2);
    }

    [Fact]
    public void NullAndNonNullSubCategory_Inequality()
    {
        SubCategory? sub1 = null;
        SubCategory? sub2 = Categories.Outcome.Food.Sub("products");

        Assert.False(sub1 == sub2);
        Assert.True(sub1 != sub2);
    }
}
