using Application;
using Domain;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class CurrencyProvider : ICurrencyProvider
{
    private readonly List<Currency> _currencies;
    public CurrencyProvider(IConfiguration configuration)
    {
        _currencies = (configuration.GetSection("CurrencyPreferences").Get<List<string>>() ?? []).Select(Currency.Parse).ToList();
    }
    
    public IReadOnlyList<Currency> GetCurrencies() => _currencies;
}