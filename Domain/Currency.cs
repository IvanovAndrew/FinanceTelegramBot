using System.Diagnostics.CodeAnalysis;

namespace Domain
{
    public class Currency
    {
        // the order is important! Rur/Amd/Gel should follow before _mapping 
        public static Currency Rur = new("RUR", "₽");
        public static Currency Amd = new("AMD", "֏");
        public static Currency Gel = new("GEL", "₾");
        public static Currency USD = new("USD", "$", "C2");
        
        private static Dictionary<string, Currency> _mapping = new()
        {
            ["драм"] = Amd, 
            ["amd"] = Amd, 
            ["rur"] = Rur, 
            ["рубл"] = Rur, 
            ["gel"] = Gel, 
            ["lari"] = Gel, 
            ["лари"] = Gel, 
            ["usd"] = USD,
            ["доллар"] = USD,
        };
        
        public readonly string Name;
        public readonly string Symbol;
        public readonly string Format;

        private Currency(string s, string symbol, string format = "C0")
        {
            Name = s;
            Symbol = symbol;
            Format = format;
        }

        public static bool TryParse(string text, [NotNullWhen(true)] out Currency? currency)
        {
            var stringToParse = text.Trim();
            currency = null;

            var availableCurrencies = GetAvailableCurrencies();
            if (text.Length == 1)
            {
                currency = availableCurrencies.FirstOrDefault(c => c.Symbol == text);

                if (currency != null)
                    return true;
            }
            
            
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
        
        public static List<Currency> GetAvailableCurrencies()
        {
            List<Currency> currencies = new List<Currency>();
            foreach (var fieldInfo in typeof(Currency).GetFields())
            {
                if (fieldInfo.FieldType == typeof(Currency))
                {
                    if (fieldInfo.GetValue(null) is Currency currency)
                    {
                        currencies.Add(currency);
                    }
                }
            }

            return currencies;
        }

        public override string ToString() => Name;
    }
}