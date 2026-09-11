namespace Domain.Services;

public interface ICurrencyExchangeOutcomeMatcher
{
    IReadOnlyList<Outcome> Match(
        IReadOnlyCollection<CurrencyExchange> exchangesFromCurrency,
        IReadOnlyDictionary<Currency, IReadOnlyCollection<Outcome>> foreignOutcomesByCurrency);
}

public class CurrencyExchangeOutcomeMatcher : ICurrencyExchangeOutcomeMatcher
{
    public IReadOnlyList<Outcome> Match(
        IReadOnlyCollection<CurrencyExchange> exchangesFromCurrency,
        IReadOnlyDictionary<Currency, IReadOnlyCollection<Outcome>> foreignOutcomesByCurrency)
    {
        var pools = foreignOutcomesByCurrency.ToDictionary(
            kv => kv.Key,
            kv => BuildPool(kv.Value));

        var result = new List<Outcome>(exchangesFromCurrency.Count);

        foreach (var exchange in exchangesFromCurrency)
        {
            var key = (exchange.Date, exchange.Shop, exchange.TargetAmount.Amount);

            if (pools.TryGetValue(exchange.TargetAmount.Currency, out var pool)
                && pool.TryGetValue(key, out var queue)
                && queue.Count > 0)
            {
                var match = queue.Dequeue();
                result.Add(new Outcome
                {
                    Date = exchange.Date,
                    Category = match.Category,
                    SubCategory = match.SubCategory,
                    Amount = exchange.SourceAmount,
                    Shop = match.Shop
                });
            }
            else
            {
                result.Add(new Outcome
                {
                    Date = exchange.Date,
                    Category = Categories.Outcome.CurrencyExchange,
                    Amount = exchange.SourceAmount,
                    Shop = exchange.Shop
                });
            }
        }

        return result;
    }
    
    private static Dictionary<(DateOnly Date, Shop? Shop, decimal Amount), Queue<Outcome>> BuildPool(
        IReadOnlyCollection<Outcome> outcomes)
    {
        var pool = new Dictionary<(DateOnly, Shop?, decimal), Queue<Outcome>>();
        foreach (var o in outcomes)
        {
            var key = (o.Date, o.Shop, o.Amount.Amount);
            if (!pool.TryGetValue(key, out var queue))
                pool[key] = queue = new Queue<Outcome>();
            queue.Enqueue(o);
        }
        return pool;
    }
}