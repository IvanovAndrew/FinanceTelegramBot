using Application.Test.Extensions;
using Domain;
using Xunit;
using Outcome = Domain.Outcome;

namespace Application.Test.BotTransitionsTest;

public class StatisticForACategoryByMonthTest
{
    [Fact]
    public async Task StatisticForACategoryWithACustomDateRange()
    {
        var scenario = await BotScenario.Start();
        
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
        await scenario.SelectCategoryByMonths();
        await scenario.WithCategory("Еда");
        await scenario.WithCustomMonth("January 2022");
        await scenario.WithAmdCurrency();
        
        // Assert
        Assert.Equal(2, scenario.MessageService.SentMessages.Count);
        var table = scenario.MessageService.SentMessages.Single(c => c.Table != null).Table;
        
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("Category", table.Subtitle);
        Assert.Contains("Еда", table.Subtitle);
        Assert.Contains("July 2023", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
    [Fact]
    public async Task StatisticForACategoryByAPeriod()
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
        await scenario.SelectCategoryByMonths();
        await scenario.WithCategory("Еда");
        await scenario.WithMonth("June 2023");
        await scenario.WithAmdCurrency();

        // Assert
        Assert.Equal(2, scenario.MessageService.SentMessages.Count);
        var table = scenario.MessageService.SentMessages.Single(c => c.Table != null).Table;
        
        Assert.NotNull(table);
        
        Assert.Equal("Statistic", table.Title);
        Assert.Contains("Category", table.Subtitle);
        Assert.Contains("Еда", table.Subtitle);
        Assert.Contains("July 2023", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
    [Fact]
    public async Task StatisticForACategoryByMonthsIsSortedChronologically()
    {
        var scenario = await BotScenario.Start();
        
        // Arrange
        await scenario.Repo.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2022, 12, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 7_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 1, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 167_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 2, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 4_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 3, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 14_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 4, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 3_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 15_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
            }, default);
        
        // Act
        await scenario.SelectStatistics();
        await scenario.SelectCategoryByMonths();
        await scenario.WithCategory("Коты");
        await scenario.WithCustomMonth("January 2022");
        await scenario.WithAmdCurrency();

        // Assert
        Assert.Equal(2, scenario.MessageService.SentMessages.Count);
        
        var table = scenario.MessageService.SentMessages.Single(c => c.Table != null).Table;
        Assert.NotNull(table);
        CollectionAssertExtension.AssertOrder(table.Rows.Select(row => row.FirstColumnValue).ToList(), "December 2022", "January 2023", "February 2023", "March 2023", "April 2023", "May 2023", "June 2023", "July 2023");
    }
}