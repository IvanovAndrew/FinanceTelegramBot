namespace Domain.Test;

public class CategoriesTest
{
    [Fact]
    public void Category_Can_Be_Found_By_Name()
    {
        Assert.NotNull(Categories.Outcome.GetCategory("Здоровье, гигиена"));
    }
    
    [Fact]
    public void Category_Can_Be_Found_By_ShortName()
    {
        Assert.NotNull(Categories.Outcome.GetCategory("Коты"));
    }
    
    [Fact]
    public void Category_Can_Be_Found_By_Code()
    {
        Assert.NotNull(Categories.Outcome.GetCategory("CulturalLife"));
    }

    [Fact]
    public void Outcome_Categories_Do_Not_Contain_Incomes()
    {
        Assert.DoesNotContain(Categories.Income.Salary, Categories.Outcome.All);
    }
    
    [Fact]
    public void Income_Categories_Do_Not_Contain_Outcomes()
    {
        Assert.DoesNotContain(Categories.Outcome.Food, Categories.Income.All);
    }
}