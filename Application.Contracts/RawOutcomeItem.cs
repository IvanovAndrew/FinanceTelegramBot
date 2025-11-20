namespace Application.Contracts;

public record RawOutcomeItem
{
    public DateOnly Date;
    public decimal Amount;
    public string Description;
    public string Currency;
}