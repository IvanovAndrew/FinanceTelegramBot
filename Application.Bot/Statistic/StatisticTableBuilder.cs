using Domain;

namespace Application.Bot.Statistic;

public record TableOptions
{
    public string? Subtitle { get; init; }
    public string? FirstColumnName { get; init; }
}

internal static class StatisticTableBuilder
{
    internal static Table BuildTable(StatisticWrapper statistic, TableOptions tableOptions)
    {
        var table = new Table()
        {
            Title = "Statistic",
            Subtitle = tableOptions.Subtitle,
            FirstColumnName = tableOptions.FirstColumnName,
            Currencies = statistic.Currencies
        };
           
        int i = 0;

        foreach (var expenseInfo in statistic.Rows)
        {
            var currencyValues = new Dictionary<Currency, Money>();
            foreach (var currency in statistic.Currencies)
            {
                currencyValues[currency] = expenseInfo[currency];
            }
                
            table.AddRow(new Row(){FirstColumnValue = expenseInfo.FirstColumn.GetString(), CurrencyValues = currencyValues});
        }

        table.AddRow(new Row());
            
        var totalValues = new Dictionary<Currency, Money>();
        foreach (var currency in statistic.Currencies)
        {
            totalValues[currency] = statistic.Total[currency];
        }
        table.AddRow(new Row {FirstColumnValue = "Total", CurrencyValues = totalValues});
        return table;
    }
}