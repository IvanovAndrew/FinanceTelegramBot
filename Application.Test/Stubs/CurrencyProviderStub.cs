using Domain;

namespace Application.Test.Stubs;

public class CurrencyProviderStub : ICurrencyProvider
{
    public IReadOnlyList<Currency> GetCurrencies()
    {
        return [Currency.AMD, Currency.RUR, Currency.USD, Currency.EUR];
    }
}