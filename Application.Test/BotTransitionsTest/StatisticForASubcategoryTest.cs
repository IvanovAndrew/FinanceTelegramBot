using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Outcome = Domain.Outcome;

namespace Application.Test.BotTransitionsTest;

public class StatisticForASubcategoryTest
{ 
    private readonly BotEngineWrapper _botEngine;
    private readonly FinanceRepositoryStub _expenseRepository;
    private readonly DateTimeServiceStub _dateTimeService;

    public StatisticForASubcategoryTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _dateTimeService, out _, out _, out _);
        _dateTimeService.SetToday(new DateOnly(2023, 7, 24));

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task StatisticForASubcategory()
    {
        // Arrange
        await _expenseRepository.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 6, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Subcategory expenses (overall)");
        await _botEngine.Proceed("Еда");
        await _botEngine.Proceed("June 2023");
        var lastMessage = await _botEngine.Proceed("AMD");

        var table = lastMessage.Table;

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
        // Arrange
        await _expenseRepository.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Subcategory expenses (overall)");
        await _botEngine.Proceed("Еда");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("March 2022");
        var lastMessage = await _botEngine.Proceed("AMD");

        var table = lastMessage.Table;
        
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