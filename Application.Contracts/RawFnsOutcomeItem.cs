namespace Application.Contracts;

public record RawFnsOutcomeItem
{
    public DateOnly Date;
    public decimal Amount;
    public string Description;
    public string? Shop;
    public string? ProductInternationalCode { get; set; }
}