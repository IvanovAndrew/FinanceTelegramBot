using Application.Test.Extensions;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using UnitTest;
using UnitTest.Extensions;
using UnitTest.Stubs;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class StatisticForASubcategoryByMonthsTest
{
    private readonly BotEngineWrapper _botEngine;
    private readonly FinanceRepositoryStub _expenseRepository;
    private readonly DateTimeServiceStub _dateTimeService;

    public StatisticForASubcategoryByMonthsTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _dateTimeService, out _, out _);
        _dateTimeService.SetToday(new DateOnly(2023, 7, 24));

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task StatisticForASubcategoryByMonths()
    {
        await _expenseRepository.SaveAllOutcomes(
        [
            new Outcome()
            {
                Date = new DateOnly(2023, 5, 22), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
            },
            new Outcome()
            {
                Date = new DateOnly(2023, 6, 23), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            },
            new Outcome()
            {
                Date = new DateOnly(2023, 7, 23), Category = "Food".AsCategory(), SubCategory = "Snacks".AsSubcategory(),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.Amd }
            },
            new Outcome()
            {
                Date = new DateOnly(2023, 7, 24), Category = "Food".AsCategory(), SubCategory = "Products".AsSubcategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            }
        ], default);
        
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Subcategory expenses (by months)");
        await _botEngine.Proceed("Food");
        await _botEngine.Proceed("Snacks");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("July 2023");
        var lastMessage = await _botEngine.Proceed("All");
        
        // Assert
        var table = lastMessage.Table;
        
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("July 2023", table.Subtitle);
        Assert.Contains("Category: Food", table.Subtitle);
        Assert.Contains("Subcategory: Snacks", table.Subtitle);
        Assert.Equivalent(new string[] {"Month", "AMD"}, table.ColumnNames);
        Assert.Equal(1000m, table.Rows.First().CurrencyValues[Currency.Amd].Amount);
        Assert.Contains("Total", table.Rows.Last().FirstColumnValue);
    }
    
    [Fact]
    public async Task StatisticForASubCategoryByMonthsIsSortedChronologically()
    {
        // Arrange
        await _expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2022, 12, 22), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 16_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 1, 22), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 17_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 2, 22), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 6_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 3, 22), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 7_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 4, 22), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 14_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 2_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 15_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);

        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Subcategory expenses (by months)");
        await _botEngine.Proceed("Food");
        await _botEngine.Proceed("Snacks");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("January 2022");
        var lastMessage = await _botEngine.Proceed("AMD");

        var table = lastMessage.Table;
        
        // Assert
        Assert.NotNull(table);
        CollectionAssertExtension.AssertOrder(table.Rows.Select(row => row.FirstColumnValue).ToList(), 
            "December 2022", "January 2023", "February 2023", "March 2023", "April 2023", "May 2023", "June 2023", "July 2023");
    }
    
    [Fact]
    public async Task StatisticForASubCategoryWithCustomDateRange()
    {
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
        await _botEngine.Proceed("Subcategory expenses (by months)");
        await _botEngine.Proceed("Food");
        await _botEngine.Proceed("Snacks");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("January 2022");
        var lastMessage = await _botEngine.Proceed("All");
        

        // Assert
        var table = lastMessage.Table;
        
        Assert.NotNull(table);
        Assert.Equal("Statistic", table.Title);
        Assert.Contains("January 2022", table.Subtitle);
        Assert.Contains("Category", table.Subtitle);
        Assert.Contains("Food", table.Subtitle);
        Assert.Contains("Subcategory", table.Subtitle);
        Assert.Contains("Snacks", table.Subtitle);
        
        Assert.Contains("July 2023", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
}