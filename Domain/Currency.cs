using System.Diagnostics.CodeAnalysis;

namespace Domain
{
    public class Currency
    {
        // the order is important! Rur/Amd/Gel should follow before _mapping 
        public static Currency RUR = new("RUR", "₽");
        public static Currency AMD = new("AMD", "֏");
        public static Currency GEL = new("GEL", "₾");
        public static Currency USD = new("USD", "$", "C2");
        public static Currency EUR = new("EUR", "€", "C2");
        public static Currency RSD = new("RSD", "din");
        public static Currency TRY = new("TRY", "\u20ba");
        
        private static Dictionary<string, Currency> _mapping = new()
        {
            ["драм"] = AMD, 
            ["рубл"] = RUR, 
            ["lari"] = GEL, 
            ["лари"] = GEL, 
            ["доллар"] = USD,
            ["euro"] = EUR,
            ["евро"] = EUR,
            ["дин"] = RSD,
            ["din"] = RSD,
            ["lir"] = TRY,
            ["лир"] = TRY,
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

        public static Currency Parse(string text)
        {
            if (TryParse(text, out var currency))
            {
                return currency;
            }

            throw new ArgumentOutOfRangeException($"Couldn't parse a currency for {text}");
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
            else
            {
                currency = availableCurrencies.FirstOrDefault(c =>
                    string.Equals(c.Name, text, StringComparison.InvariantCultureIgnoreCase));
                    
                if (currency != null)
                {
                    return true;
                }
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