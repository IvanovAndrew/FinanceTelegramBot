using System.Globalization;
using System.Text.RegularExpressions;

namespace Domain
{
    public class Money : IComparable<Money>, IEquatable<Money>
    {
        public Currency Currency { get; init; }
        public decimal Amount { get; init; }

        public static Money Zero(Currency currency) => new Money() { Currency = currency, Amount = 0m };
        
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
        
        public static Money operator /(Money money, int denominator)
        {
            var result = Math.Round(money.Amount / denominator, 2, MidpointRounding.AwayFromZero);
            
            return new Money { Currency = money.Currency, Amount = result };
        }

        public static Result<Money> Parse(string s, Currency currency, IFormatProvider formatProvider)
        {
            if (decimal.TryParse(s, NumberStyles.Currency, formatProvider, out var amount))
            {
                return Result<Money>.Success(new Money() { Currency = currency, Amount = amount });
            }
            
            return Result<Money>.Failure("Couldn't parse amount");
        }
        
        public static Result<Money> Parse(string? text)
        {
            Regex amountRegex = new Regex(@"((\d+\s?)+\.?\d*)");

            var str = text.Replace(",", ".");
            Match match = amountRegex.Match(str);

            if (!match.Success)
            {
                return Result<Money>.Failure("Unknown value");
            }

            var numberPart = match.Groups[0].Value;

            Currency? currency;
            if (!Currency.TryParse(str.Replace(numberPart, ""), out currency))
            {
                return Result<Money>.Failure("Missing currency");
            }

            if (!decimal.TryParse(numberPart.Replace(" ", ""), out var amount))
            {
                return Result<Money>.Failure("Missing amount");
            }

            return Result<Money>.Success(new Money { Amount = amount, Currency = currency });
        }
    
        public override string ToString()
        {
            var culture = CultureInfo.GetCultureInfo("ru-RU");
            var numberFormat = (NumberFormatInfo)culture.NumberFormat.Clone();
            numberFormat.CurrencySymbol = Currency.Symbol;
            
            return Amount.ToString(Currency.Format, numberFormat);
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