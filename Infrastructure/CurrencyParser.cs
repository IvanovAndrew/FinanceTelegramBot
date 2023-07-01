using Domain;

namespace Infrastructure
{
    public class CurrencyParser : ICurrencyParser
    {
        public CurrencyParser()
        {
        }

        public bool TryParse(string text, out Currency? currency)
        {
            var stringToParse = text.Trim();
            currency = null;
        
            if (stringToParse.Contains("драм", StringComparison.InvariantCultureIgnoreCase) || stringToParse.Contains("amd", StringComparison.InvariantCultureIgnoreCase))
            {
                currency = Currency.Amd;
                return true;
            }
        
            if (stringToParse.Contains("рубл", StringComparison.InvariantCultureIgnoreCase) || stringToParse.Contains("rur", StringComparison.InvariantCultureIgnoreCase))
            {
                currency = Currency.Rur;
                return true;
            }

            return false;
        }
    }
}