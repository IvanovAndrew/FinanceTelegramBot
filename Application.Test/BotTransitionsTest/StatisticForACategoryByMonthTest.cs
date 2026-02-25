using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Extensions;
using Xunit;
using Outcome = Domain.Outcome;

namespace Application.Test.BotTransitionsTest;

public class StatisticForACategoryByMonthTest
{
    private readonly BotEngineWrapper _botEngine;
    private readonly FinanceRepositoryStub _expenseRepository;
    private readonly DateTimeServiceStub _datetimeService;
    private readonly MessageServiceMock _messageServiceMock;
    

    public StatisticForACategoryByMonthTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _datetimeService, out _messageServiceMock, out _, out _);
        _datetimeService.SetToday(new DateOnly(2023, 7, 24));

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task StatisticForACategoryWithACustomDateRange()
    {
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
        await _botEngine.Proceed("Category expenses (by months)");
        await _botEngine.Proceed("Еда");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("January 2022");
        await _botEngine.Proceed("AMD");
        
        // Assert
        Assert.Equal(2, _messageServiceMock.SentMessages.Count);
        var table = _messageServiceMock.SentMessages.Single(c => c.Table != null).Table;
        
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
        await _botEngine.Proceed("Category expenses (by months)");
        await _botEngine.Proceed("Еда");
        await _botEngine.Proceed("June 2023");
        await _botEngine.Proceed("AMD");

        // Assert
        Assert.Equal(2, _messageServiceMock.SentMessages.Count);
        var table = _messageServiceMock.SentMessages.Single(c => c.Table != null).Table;
        
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
        // Arrange
        await _expenseRepository.SaveAllOutcomes(
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
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Category expenses (by months)");
        await _botEngine.Proceed("Коты");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("January 2022");
        await _botEngine.Proceed("AMD");

        // Assert
        Assert.Equal(2, _messageServiceMock.SentMessages.Count);
        
        var table = _messageServiceMock.SentMessages.Single(c => c.Table != null).Table;
        Assert.NotNull(table);
        CollectionAssertExtension.AssertOrder(table.Rows.Select(row => row.FirstColumnValue).ToList(), "December 2022", "January 2023", "February 2023", "March 2023", "April 2023", "May 2023", "June 2023", "July 2023");
    }
}