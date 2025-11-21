using Domain;

namespace Application;

public interface ICurrencyProvider
{
    IReadOnlyList<Currency> GetCurrencies();
}