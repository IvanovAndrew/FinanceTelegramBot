using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using UnitTest;
using UnitTest.Stubs;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class StatisticForASubcategoryTest
{ 
    private readonly BotEngineWrapper _botEngine;
    private readonly FinanceRepositoryStub _expenseRepository;
    private readonly DateTimeServiceStub _dateTimeService;

    public StatisticForASubcategoryTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _dateTimeService, out _, out _);
        _dateTimeService.SetToday(new DateOnly(2023, 7, 24));

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task StatisticForASubcategory()
    {
        // Arrange
        await _expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats".AsCategory(), Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats".AsCategory(), Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food".AsCategory(), SubCategory = "Snacks".AsSubcategory(), Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food".AsCategory(), SubCategory = "Products".AsSubcategory(), Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Subcategory expenses (overall)");
        await _botEngine.Proceed("Food");
        await _botEngine.Proceed("July 2022");
        var lastMessage = await _botEngine.Proceed("AMD");

        var table = lastMessage.Table;

        // Assert
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("Expenses from July 2022", table.Subtitle);
        Assert.Contains("Category: Food", table.Subtitle);
        Assert.Contains("Subcategory", table.FirstColumnName);
        Assert.Contains("Snacks", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Products", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
    [Fact]
    public async Task StatisticByASubcategoryWithCustomDateRange()
    {
        // Arrange
        await _expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats".AsCategory(), Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats".AsCategory(), Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food".AsCategory(), SubCategory = "Snacks".AsSubcategory(), Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food".AsCategory(), SubCategory = "Products".AsSubcategory(), Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Subcategory expenses (overall)");
        await _botEngine.Proceed("Food");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("March 2022");
        var lastMessage = await _botEngine.Proceed("All");

        var table = lastMessage.Table;
        
        // Assert
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("Expenses from March 2022", table.Subtitle);
        Assert.Contains("Category: Food", table.Subtitle);
        Assert.Contains("Snacks", table.Rows.Select(c => c.FirstColumnValue));
        Assert.Contains("Products", table.Rows.Select(c => c.FirstColumnValue));
        Assert.Equal(1_000m, table.Rows.First(c => c.FirstColumnValue == "Snacks").CurrencyValues[Currency.Amd].Amount);
        Assert.Contains("Total", table.Rows.Select(c => c.FirstColumnValue));
    }
}