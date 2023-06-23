using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Domain
{
    public class Money : IComparable<Money>
    {
        public Currency Currency { get; init; }
        public decimal Amount { get; init; }

        public static Money operator +(Money one, Money two)
        {
            if (one.Currency != two.Currency)
                throw new InvalidOperationException("Money should have the same currency!");

            return new Money { Currency = one.Currency, Amount = one.Amount + two.Amount };
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
    
        public override string ToString() => $"{Currency}{Amount}";
        public string ToString(string format) => $"{Currency}{Amount.ToString(format)}";

        public int CompareTo(Money? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Amount.CompareTo(other.Amount);
        }
    }
}