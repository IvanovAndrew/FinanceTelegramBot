using Domain;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class StatisticForADayTest
{
    [Fact]
    public async Task Statistic_For_Yesterday()
    {
        var scenario = await BotScenario.Start();
        scenario.Time.SetToday(new DateOnly(2023, 7, 24)); 
        
        await scenario.Repo.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        
        // Act
        await scenario.SelectStatistics();
        await scenario.SelectDayStatistics();
        await scenario.WithDateYesterday();
        await scenario.WithAmdCurrency();

        // Assert
        var table = scenario.LastMessage.Table;
        
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("23 July 2023", table?.Subtitle ?? string.Empty);
        Assert.Equivalent(new []{"Category", "AMD"}, table?.ColumnNames);
        Assert.Contains("Домашние животные", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Еда", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
    [Fact]
    public async Task StatisticForADayAllowsToChooseBetweenTodayYesterdayAndEnterCustomDate()
    {
        var scenario = await BotScenario.Start();
        
        // Act
        await scenario.SelectStatistics();
        await scenario.SelectDayStatistics();

        // Assert
        Assert.NotEmpty(scenario.LastMessage.Options);
        
        var buttons = scenario.LastMessage.Options.Select(_ => _.Text);
        Assert.Contains("Today", buttons);
        Assert.Contains("Yesterday", buttons);
        Assert.Contains("Another day", buttons);
    }
    
    [Fact]
    public async Task Statistic_For_A_Custom_Day()
    {
        var scenario =  await BotScenario.Start();
        
        await scenario.Repo.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        
        // Act
        await scenario.SelectStatistics();
        await scenario.SelectDayStatistics();
        await scenario.WithDate("22 July 2023");
        await scenario.WithAmdCurrency();

        // Assert
        var table = scenario.LastMessage.Table;
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("22 July 2023", table.Subtitle);
        Assert.Contains("Category", table.FirstColumnName);
        Assert.Contains("Домашние животные", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Equal(new Money(){Amount = 10_000, Currency = Currency.AMD}, table.Rows.Select(r => r.CurrencyValues[Currency.AMD]).First());
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
}