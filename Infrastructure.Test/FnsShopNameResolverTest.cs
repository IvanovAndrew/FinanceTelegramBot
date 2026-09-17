using Application.Contracts.FNS;
using Domain;
using Infrastructure.Fns;

namespace Infrastructure.Test;

public class FnsShopNameResolverTest
{
    [Theory]
    [InlineData("Общество с ограниченной ответственностью \"Интернет Решения\"")]
    [InlineData("ОБЩЕСТВО С ОГРАНИЧЕННОЙ ОТВЕТСТВЕННОСТЬЮ \"ИНТЕРНЕТ РЕШЕНИЯ\"")]
    public void Ozon(string userName)
    {
        var shop = GetShopName(userName, "ozon.ru");
        
        Assert.Equal("Озон", shop?.Name);
    }
    
    [Fact]
    public void FixPrice()
    {
        var shop = GetShopName("ООО \"БЭСТ ПРАЙС\"", "МАГАЗИН №4087");
        
        Assert.Equal("Fix Price", shop?.Name);
    }
    
    [Fact]
    public void Aeroflot()
    {
        var shop = GetShopName("ПАО \"Аэрофлот\"", "https://www.aeroflot.ru");
        
        Assert.Equal("Аэрофлот", shop?.Name);
    }

    private Shop? GetShopName(string user, string retailPlace)
    {
        var fnsCheckInfo = new FnsCheckInfo(){User = user, RetailPlace = retailPlace};

        return new FnsShopNameResolver().Resolve(fnsCheckInfo);
    }
}