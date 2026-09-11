namespace Domain;

public class ShopExpenses
{
    public Shop Shop { get; init; }
    public DateOnly Date { get; init; }
    public Currency Currency => Outcomes.First().Amount.Currency;
    
    public Money Total => Outcomes.Aggregate(Money.Zero(Currency), (acc, m) => acc + m.Amount);
    
    
    public IReadOnlyCollection<Outcome> Outcomes { get; init; }
}