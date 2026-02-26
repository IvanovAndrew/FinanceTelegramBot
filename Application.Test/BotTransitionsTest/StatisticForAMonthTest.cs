using Domain;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class StatisticForAMonthTest
{
    [Fact]
    public async Task StatisticForAMonthAllowsToChooseBetweenCurrentPreviousAndEnterCustomMonth()
    {
        var scenario = await BotScenario.Start();
        scenario.Time.SetToday(new DateOnly(2023, 7, 24));
        
        // Arrange
        await scenario.Repo.SaveAllOutcomes(
            new List<Outcome>()
            {
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
                },
            }, default);

        // Act
        await scenario.SelectStatistics();
        await scenario.SelectMonthStatistics();

        // Assert
        Assert.NotNull(scenario.LastMessage.Options);
        
        var buttons = scenario.LastMessage.Options.Select(_ => _.Text);
        
        Assert.Contains("July 2023", buttons);
        Assert.Contains("June 2023", buttons);
        Assert.Contains("Another month", buttons);
    }

    [Fact]
    public async Task StatisticForACustomMonth()
    {
        var scenario = await BotScenario.Start();
        
        await scenario.Repo.SaveAllOutcomes(
            new List<Outcome>()
            {
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
                },
            }, default);

        // Act
        await scenario.SelectStatistics();
        await scenario.SelectMonthStatistics();
        await scenario.WithCustomMonth("May 2023");
        await scenario.WithAmdCurrency();

        var table = scenario.MessageService.SentMessages.Single().Table;

        // Assert
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("May 2023", table.Subtitle);
        Assert.Contains("Category", table.ColumnNames);
        Assert.Contains("Домашние животные", table.Rows.Select(c => c.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(c => c.FirstColumnValue));
    }
}