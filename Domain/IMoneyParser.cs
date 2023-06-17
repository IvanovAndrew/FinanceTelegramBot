namespace Domain
{
    public interface IMoneyParser
    {
        bool TryParse(string text, out Money? money);
    }
}