using Domain;

public class MoneyAdditionException : DomainException
{
    public override string Message { get; }

    public MoneyAdditionException(Money one, Money two) : base()
    {
        Message = $"Money should have the same currency! We have {one} and {two}";
    }
}