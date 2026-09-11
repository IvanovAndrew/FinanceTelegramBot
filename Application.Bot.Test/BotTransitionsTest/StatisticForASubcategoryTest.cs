using Domain;
using Xunit;
using Outcome = Domain.Outcome;

namespace Application.Test.BotTransitionsTest;

public class StatisticForASubcategoryTest
{ 
    [Fact]
    public async Task StatisticForASubcategory()
    {
        var scenario = await BotScenario.Start();
        
        // Arrange
        await scenario.Repo.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 6, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        
        // Act
        await scenario.SelectStatistics();
        await scenario.SelectSubcategoryOverall();
        await scenario.WithCategory("Еда");
        await scenario.WithMonth("June 2023");
        await scenario.WithAmdCurrency();

        var table = scenario.LastMessage.Table;

        // Assert
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("Expenses from June 2023", table.Subtitle);
        Assert.Contains("Category: Еда", table.Subtitle);
        Assert.Contains("Subcategory", table.FirstColumnName);
        Assert.Contains("Перекусы", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Продукты", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
    [Fact]
    public async Task StatisticByASubcategoryWithCustomDateRange()
    {
        var scenario = await BotScenario.Start();
        
        // Arrange
        await scenario.Repo.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        
        // Act
        await scenario.SelectStatistics();
        await scenario.SelectSubcategoryOverall();
        await scenario.WithCategory("Еда");
        await scenario.WithCustomMonth("March 2022");
        await scenario.WithAmdCurrency();

        var table = scenario.LastMessage.Table;
        
        // Assert
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("Expenses from March 2022", table.Subtitle);
        Assert.Contains("Category: Еда", table.Subtitle);
        Assert.Contains("Перекусы", table.Rows.Select(c => c.FirstColumnValue));
        Assert.Contains("Продукты", table.Rows.Select(c => c.FirstColumnValue));
        Assert.Equal(1_000m, table.Rows.First(c => c.FirstColumnValue == "Перекусы").CurrencyValues[Currency.AMD].Amount);
        Assert.Contains("Total", table.Rows.Select(c => c.FirstColumnValue));
    }
}