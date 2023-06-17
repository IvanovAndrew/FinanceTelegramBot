using System.Text.RegularExpressions;

namespace Domain
{
    public class MoneyParser : IMoneyParser
    {
        private readonly ICurrencyParser _currencyParser;
        public MoneyParser(ICurrencyParser currencyParser)
        {
            _currencyParser = currencyParser;
        }
    
        public bool TryParse(string? text, out Money? money)
        {
            money = null;
        
            Regex amountRegex = new Regex(@"((\d+\s?)+\.?\d*)");

            var str = text.Replace(",", ".");
            Match match = amountRegex.Match(str);
        
            if (!match.Success) return false;

            var numberPart = match.Groups[0].Value;

            Currency? currency;
            if (!_currencyParser.TryParse(str.Replace(numberPart, ""), out currency))
            {
                return false;
            }

            if (decimal.TryParse(numberPart.Replace(" ", ""), out var amount))
            {
                money = new Money { Amount = amount, Currency = currency };
                return true;
            }

            return false;
        }
    }
}