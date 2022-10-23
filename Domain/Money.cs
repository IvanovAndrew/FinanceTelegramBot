using System.Globalization;
using System.Text.RegularExpressions;

namespace Domain;

public class Currency
{
    private readonly string Name;

    public static Currency Rur = new Currency("RUR");
    public static Currency Amd = new Currency("AMD");
    
    private Currency(string s)
    {
        Name = s;
    }

    public override string ToString() => Name;
}

public class Money
{
    public Currency Currency { get; init; }
    public decimal Amount { get; init; }

    public override string ToString() => $"{Amount}{Currency}";
}

public interface IMoneyParser
{
    bool TryParse(string text, out Money? money);
}

public interface ICurrencyParser
{
    bool TryParse(string text, out Currency? currency);
}

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