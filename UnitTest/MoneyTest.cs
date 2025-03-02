using Domain;
using Xunit;

namespace UnitTest;

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
}