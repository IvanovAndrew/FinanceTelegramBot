namespace Domain.Test;

public class CurrencyTest
{
    [Theory]
    [InlineData("$")]
    [InlineData("€")]
    [InlineData("₽")]
    [InlineData("֏")]
    [InlineData("₾")]
    [InlineData("\u20ba")]
    [InlineData("din")]
    public void GetCurrencyBySymbol(string symbol)
    {
        Assert.True(Currency.TryParse(symbol, out var currency));
        Assert.Equal(symbol, currency.Symbol);
    }
    
    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("AMD")]
    [InlineData("RUR")]
    [InlineData("GEL")]
    [InlineData("TRY")]
    [InlineData("RSD")]
    public void GetCurrencyByName(string name)
    {
        Assert.True(Currency.TryParse(name, out var currency));
        Assert.Equal(name, currency.Name);
    }
}