using Domain;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class StatisticBalanceTest 
{
    [Fact]
    public async Task Balance_Since_Previous_Month()
    {
        var scenario = await BotScenario.Start();
        
        await scenario.Repo.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        await scenario.Repo.SaveIncome(CreateSalary(), default);
        
        // Act
        await scenario.SelectStatistics();
        await scenario.SelectBalance();
        await scenario.WithMonth("June 2023");
        await scenario.WithAmdCurrency();

        // Assert
        var table = scenario.MessageService.SentMessages.First(t => t.Table != null)?.Table;
        
        Assert.NotNull(table);
        Assert.Contains("Balance", table.Title);
        Assert.Contains("June 2023", table.Subtitle);
        Assert.Equal(["Balance", "AMD"], table?.ColumnNames);
        Assert.Contains("Income", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Outcome", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
        
    [Fact]
    public async Task Balance_From_Custom_Month()
    {
        var scenario = await BotScenario.Start();
        scenario.SalaryDayService.SalaryDay = new DateOnly(2023, 8, 1);
        await scenario.Repo.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        await scenario.Repo.SaveIncome(CreateSalary(), default);
        
        // Act
        await scenario.SelectStatistics();
        await scenario.SelectBalance();
        await scenario.WithCustomMonth("January 2023");
        await scenario.WithAmdCurrency();

        // Assert
        var table = scenario.MessageService.SentMessages.First(t => t.Table != null)?.Table;
        
        Assert.NotNull(table);
        Assert.Contains("Balance", table.Title);
        Assert.Contains("January 2023", table.Subtitle);
        Assert.Equal(["Balance", "AMD"], table?.ColumnNames);
        Assert.Contains("Income", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Outcome", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
    [Fact]
    public async Task StatisticFromJanuary_Messages()
    {
        var scenario = await BotScenario.Start();
        
        await scenario.Repo.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        await scenario.Repo.SaveIncome(CreateSalary(), default);
        
        // Act
        await scenario.SelectStatistics();
        await scenario.SelectBalance();
        await scenario.WithCustomMonth("January 2023");
        await scenario.WithAmdCurrency();

        // Assert
        Assert.Equal(3, scenario.MessageService.SentMessages.Count);

        var (firstMessage, secondMessage) = (scenario.MessageService.SentMessages[0], scenario.MessageService.SentMessages[1]);

        Assert.NotNull(secondMessage.Table);
    }

    private Income CreateSalary()
    {
        return new Income() { 
            Date = new DateOnly(2023, 7, 1), 
            Amount = new Money(){Amount = 1000, Currency = Currency.AMD}, 
            Category = Categories.Income.Salary
        };
    }
}