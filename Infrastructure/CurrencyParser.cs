using Domain;

namespace Infrastructure
{
    public class CurrencyParser : ICurrencyParser
    {
        private Dictionary<string, Currency> _mapping = new()
        {
            ["драм"] = Currency.Amd, 
            ["amd"] = Currency.Amd, 
            ["rur"] = Currency.Rur, 
            ["рубл"] = Currency.Rur, 
            ["gel"] = Currency.Gel, 
            ["lari"] = Currency.Gel, 
            ["лари"] = Currency.Gel, 
        };
        
        public CurrencyParser()
        {
        }

        public bool TryParse(string text, out Currency? currency)
        {
            var stringToParse = text.Trim();
            currency = null;
            
            foreach (var (mask, mappedCurrency) in _mapping)
            {
                if (stringToParse.Contains(mask, StringComparison.InvariantCultureIgnoreCase))
                {
                    currency = mappedCurrency;
                    return true;
                }
            }
        
            return false;
        }
    }
}