namespace Domain.Test;

using System.Collections.Generic;
using Domain;
using Xunit;

public class CategoryEqualityTests
{
    [Fact]
    public void Categories_WithSameName_AreEqual()
    {
        var cat1 = new Category { Name = "Food" };
        var cat2 = new Category { Name = "Food" };

        Assert.True(cat1 == cat2);
        Assert.False(cat1 != cat2);
        Assert.Equal(cat1, cat2);
        Assert.Equal(cat1.GetHashCode(), cat2.GetHashCode());
    }

    [Fact]
    public void SubCategories_WithSameName_AreEqual()
    {
        var sub1 = new SubCategory { Name = "Groceries" };
        var sub2 = new SubCategory { Name = "Groceries" };

        Assert.True(sub1 == sub2);
        Assert.False(sub1 != sub2);
        Assert.Equal(sub1, sub2);
        Assert.Equal(sub1.GetHashCode(), sub2.GetHashCode());
    }

    [Fact]
    public void HashSet_Contains_MatchingCategory()
    {
        var set = new HashSet<Category> { new Category { Name = "Transport" } };
        var test = new Category { Name = "Transport" };

        Assert.Contains(test, set);
    }

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
        Category? cat2 = new Category { Name = "Books" };

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
        SubCategory? sub2 = new SubCategory { Name = "Books" };

        Assert.False(sub1 == sub2);
        Assert.True(sub1 != sub2);
    }
}
