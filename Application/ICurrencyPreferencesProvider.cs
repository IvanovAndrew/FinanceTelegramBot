using Domain;

namespace Application;

public interface ICurrencyPreferencesProvider
{
    IReadOnlyList<Currency> Currencies { get; }
}