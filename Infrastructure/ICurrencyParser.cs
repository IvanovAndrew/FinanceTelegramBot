using Domain;

namespace Infrastructure
{
    public interface ICurrencyParser
    {
        bool TryParse(string text, out Currency? currency);
    }
}