using Application.AddMoneyTransfer;
using Domain;

namespace Application;

public class StatisticWrapper
{
    public List<RowWrapper> Rows { get; set; } = new();
    public IReadOnlyDictionary<Currency, Money> Total { get; init; } 
    public IReadOnlyList<Currency> Currencies { get; init; }
}

public class RowWrapper
{
    public IFirstColumnValue FirstColumn { get; set; } = default!;
    public Dictionary<Currency, Money> CurrencyValues { get; set; } = new();
    public Money this[Currency currency] => CurrencyValues[currency];
}

public static class StatisticMapper
{
    public static StatisticWrapper Map<T>(Statistic<T> statistic, IFirstColumnFactory<T> columnFactory)
    {
        return new StatisticWrapper
        {
            Currencies = statistic.Currencies,
            Total = statistic.Currencies.ToDictionary(
                c => c,
                c => statistic.Total[c] // Вот здесь!
            ),
            Rows = statistic.Rows.Select(row => new RowWrapper
            {
                FirstColumn = columnFactory.Create(row.Row),
                CurrencyValues = statistic.Currencies.ToDictionary(
                    c => c,
                    c => row[c]
                )
            }).ToList()
        };
    }
}
