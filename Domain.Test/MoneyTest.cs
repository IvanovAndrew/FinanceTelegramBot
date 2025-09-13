namespace Domain.Test;

public class MoneyTest
{
    [Fact]
    public void AmdFormat()
    {
        var money = new Money() { Currency = Currency.Amd, Amount = 1000 };

        var s = money.ToString();
        Assert.Equal($"1{TestConstants.NBSP}000 ֏", s);
    }
    
    [Fact]
    public void UsdFormat()
    {
        var money = new Money() { Currency = Currency.USD, Amount = 1.19m };

        var s = money.ToString();
        Assert.Equal($"1,19 $", s);
    }

    [Theory]
    [InlineData("1€", 1, "€")]
    [InlineData("1.5$", 1.5, "$")]
    [InlineData("1000֏", 1000, "֏")]
    public void ParseMoney(string input, decimal expectedValue, string expectedCurrency)
    {
        var parseResult = Money.Parse(input);
        
        Assert.True(parseResult.IsSuccess);
        
        Assert.Equal(expectedValue, parseResult.Value.Amount);
        Assert.Equal(expectedCurrency, parseResult.Value.Currency.Symbol);
    }
}