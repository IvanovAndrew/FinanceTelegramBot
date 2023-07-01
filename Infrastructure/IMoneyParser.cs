using Domain;

namespace Infrastructure
{
    public interface IMoneyParser
    {
        bool TryParse(string text, out Money? money);
    }
}