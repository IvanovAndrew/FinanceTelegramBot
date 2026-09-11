namespace Domain;

public readonly record struct MonthlyBalance(YearMonth Month, Balance Balance);


public readonly struct Balance(Money income, Money outcome)
{
    public readonly Money Income = income;
    public readonly Money Outcome = outcome;
    public Money Saldo => Income - Outcome;
    
    public static Balance operator +(Balance first, Balance second)
    {
        return new Balance(first.Income + second.Income, first.Outcome + second.Outcome);
    }

    public override string ToString()
    {
        return $"Income = {Income} Outcome = {Outcome}";
    }
}

public class BalancePeriod(
    IEnumerable<IMoneyTransfer> incomes,
    IEnumerable<IMoneyTransfer> outcomes,
    Currency currency)
{
    private readonly ILookup<YearMonth, Money> _incomesByMonth = GroupByMonth(incomes, currency);
    private readonly ILookup<YearMonth, Money> _outcomesByMonth = GroupByMonth(outcomes, currency);
    
    private static ILookup<YearMonth, Money> GroupByMonth(IEnumerable<IMoneyTransfer> transfers, Currency currency) =>
        transfers
            .Where(t => t.Amount.Currency == currency)
            .ToLookup(t => YearMonth.From(t.Date), t => t.Amount);
    
    public IReadOnlyList<MonthlyBalance> ByMonths(MonthRange monthRange)
    {
        var result = new List<MonthlyBalance>();

        for (var month = monthRange.From; month <= monthRange.To; month = month.Next())
        {
            result.Add(new MonthlyBalance(
                month,
                new Balance(Sum(_incomesByMonth, month), Sum(_outcomesByMonth, month))));
        }

        return result;
    }

    private Money Sum(ILookup<YearMonth, Money> byMonth, YearMonth month) =>
        byMonth[month].Aggregate(Money.Zero(currency), (acc, amount) => acc + amount);
}