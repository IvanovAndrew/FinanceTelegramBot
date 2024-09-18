using Domain;
using Xunit;

namespace UnitTest;

public class MoneyTest
{
    [Fact]
    public void Format()
    {
        var money = new Money() { Currency = Currency.Amd, Amount = 1000 };

        var s = money.ToString();
        Assert.Equal($"1{TestConstants.NBSP}000,00 ֏", s);
    }
}