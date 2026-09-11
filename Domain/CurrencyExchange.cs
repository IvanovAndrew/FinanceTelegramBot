namespace Domain;

public class CurrencyExchange
{
    public DateOnly Date { get; init; }
    public Shop? Shop { get; init; }
    public string? Description { get; init; }
    
    public Money SourceAmount { get; init; }
    public Money TargetAmount { get; init; }
}