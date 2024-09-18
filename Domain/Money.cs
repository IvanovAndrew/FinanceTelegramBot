using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Domain;

namespace Domain
{
    public class Money : IComparable<Money>, IEquatable<Money>
    {
        public Currency Currency { get; init; }
        public decimal Amount { get; init; }

        public static Money operator +(Money one, Money two)
        {
            if (one.Currency != two.Currency)
                throw new MoneyAdditionException(one, two);

            return new Money { Currency = one.Currency, Amount = one.Amount + two.Amount };
        }
        
        public static Money operator -(Money one, Money two)
        {
            if (one.Currency != two.Currency)
                throw new MoneyAdditionException(one, two);

            return new Money { Currency = one.Currency, Amount = one.Amount - two.Amount };
        }

        public static bool TryParse(string s, Currency currency, IFormatProvider formatProvider, [NotNullWhen(true)] out Money? money)
        {
            if (decimal.TryParse(s, NumberStyles.Currency, formatProvider, out var amount))
            {
                money = new Money() { Currency = currency, Amount = amount };
                return true;
            }

            money = null;
            return false;
        }
        
        public static bool TryParse(string? text, [NotNullWhen(true)] out Money? money)
        {
            money = null;
        
            Regex amountRegex = new Regex(@"((\d+\s?)+\.?\d*)");

            var str = text.Replace(",", ".");
            Match match = amountRegex.Match(str);
        
            if (!match.Success) return false;

            var numberPart = match.Groups[0].Value;

            Currency? currency;
            if (!Currency.TryParse(str.Replace(numberPart, ""), out currency))
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
    
        public override string ToString()
        {
            var culture = CultureInfo.GetCultureInfo("ru-RU");
            var numberFormat = (NumberFormatInfo)culture.NumberFormat.Clone();
            numberFormat.CurrencySymbol = Currency.Symbol;
            
            return Amount.ToString("C0", numberFormat);
        }

        public int CompareTo(Money? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Amount.CompareTo(other.Amount);
        }

        public bool Equals(Money? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Currency.Equals(other.Currency) && Amount == other.Amount;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Money)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Currency, Amount);
        }
    }
}

public class MoneyAdditionException : Exception
{
    public override string Message { get; }

    public MoneyAdditionException(Money one, Money two) : base()
    {
        Message = $"Money should have the same currency! We have {one} and {two}";
    }
}