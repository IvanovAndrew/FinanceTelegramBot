using Application.Test.Extensions;
using Domain;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class StatisticForASubcategoryByMonthsTest
{
    [Fact]
    public async Task Subcategory_By_Month_With_Custom_Month()
    {
        var scenario = await BotScenario.Start();
        
        await scenario.Repo.SaveAllOutcomes(
        [
            new Outcome()
            {
                Date = new DateOnly(2023, 5, 22), Category = Categories.Outcome.Pets,
                Amount = new Money() { Amount = 10_000m, Currency = Currency.AMD }
            },
            new Outcome()
            {
                Date = new DateOnly(2023, 6, 23), Category = Categories.Outcome.Pets,
                Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
            },
            new Outcome()
            {
                Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.AMD }
            },
            new Outcome()
            {
                Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
            }
        ], default);
        
        
        // Act
        await scenario.SelectStatistics();
        await scenario.SelectSubcategoryByMonths();
        await scenario.WithCategory("Еда");
        await scenario.WithSubCategory("Перекусы");
        await scenario.WithCustomMonth("July 2023");
        await scenario.WithAmdCurrency();
        
        // Assert
        Assert.Equal(2, scenario.MessageService.SentMessages.Count);
        var table = scenario.MessageService.SentMessages.Single(m => m.Table != null).Table;
        
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("July 2023", table.Subtitle);
        Assert.Contains("Category: Еда", table.Subtitle);
        Assert.Contains("Subcategory: Перекусы", table.Subtitle);
        Assert.Equivalent(new string[] {"Month", "AMD"}, table.ColumnNames);
        Assert.Equal(1000m, table.Rows.First().CurrencyValues[Currency.AMD].Amount);
        Assert.Contains("Total", table.Rows.Last().FirstColumnValue);
    }
    
    [Fact]
    public async Task StatisticForASubCategoryByMonthsIsSortedChronologically()
    {
        var scenario = await BotScenario.Start();
        
        // Arrange
        await scenario.Repo.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2022, 12, 22), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 16_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 1, 22), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 17_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 2, 22), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 6_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 3, 22), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 7_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 4, 22), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 14_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 2_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 15_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);

        // Act
        await scenario.SelectStatistics();
        await scenario.SelectSubcategoryByMonths();
        await scenario.WithCategory("Еда");
        await scenario.WithSubCategory("Перекусы");
        await scenario.WithCustomMonth("January 2022");
        await scenario.WithAmdCurrency();

        // Assert
        Assert.Equal(2, scenario.MessageService.SentMessages.Count);
        
        var table = scenario.MessageService.SentMessages.Single(c => c.Table != null).Table;
        Assert.NotNull(table);
        CollectionAssertExtension.AssertOrder(table.Rows.Select(row => row.FirstColumnValue).ToList(), 
            "December 2022", "January 2023", "February 2023", "March 2023", "April 2023", "May 2023", "June 2023", "July 2023");
    }
    
    [Fact]
    public async Task StatisticForASubCategoryWithCustomDateRange()
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
        await scenario.SelectSubcategoryByMonths();
        await scenario.WithCategory("Еда");
        await scenario.WithSubCategory("Перекусы");
        await scenario.WithCustomMonth("January 2022");
        await scenario.WithAmdCurrency();
        

        // Assert
        Assert.Equal(2, scenario.MessageService.SentMessages.Count);
        Assert.Single(scenario.MessageService.SentMessages, m => m.Table != null);
        
        var table = scenario.MessageService.SentMessages.Single(c => c.Table != null).Table;
        
        Assert.Equal("Statistic", table.Title);
        Assert.Contains("January 2022", table.Subtitle);
        Assert.Contains("Category", table.Subtitle);
        Assert.Contains("Еда", table.Subtitle);
        Assert.Contains("Subcategory", table.Subtitle);
        Assert.Contains("Перекусы", table.Subtitle);
        
        Assert.Contains("July 2023", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
}