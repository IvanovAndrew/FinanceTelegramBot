using Domain;

public class MoneyAdditionException(Money one, Money two) : DomainException
{
    public override string Message { get; } = $"Money should have the same currency! We have {one} and {two}";
}