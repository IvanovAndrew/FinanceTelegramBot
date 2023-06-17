namespace Domain
{
    public interface ICurrencyParser
    {
        bool TryParse(string text, out Currency? currency);
    }
}