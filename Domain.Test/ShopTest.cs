namespace Domain.Test;

public class ShopTest
{
    [Theory]
    [InlineData("ucom", "Ucom")]
    [InlineData("LeoVet", "Leo Vet")]
    [InlineData("Rau  Pool", "RAU Pool ")]
    public void Shop_Ignores_Case_And_Whitespace(string firstName, string secondName)
    {
        // Act
        var firstShop = Shop.Create(firstName);
        var secondShop = Shop.Create(secondName);

        // Assert
        Assert.Equal(firstShop, secondShop);
    }
}