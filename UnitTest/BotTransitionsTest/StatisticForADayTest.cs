using Domain;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Extensions;
using UnitTest.Stubs;
using Xunit;

namespace UnitTest.BotTransitionsTest;

public class StatisticForADayTest
{
    private readonly BotEngineWrapper _botEngine;
    private readonly DateTimeServiceStub _dateTimeService;
    private readonly MessageServiceMock _messageService;
    private readonly FinanceRepositoryStub _expenseRepository;

    public StatisticForADayTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _dateTimeService, out _messageService, out _);

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task StatisticForADay()
    {
        _dateTimeService.SetToday(new DateOnly(2023, 7, 24)); 
        
        await _expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Day expenses (by categories)");
        await _botEngine.Proceed("Yesterday");
        var lastMessage = await _botEngine.Proceed("All");

        // Assert
        var table = lastMessage.Table;
        
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("23 July 2023", table?.Subtitle ?? string.Empty);
        Assert.Equivalent(new []{"Category", "AMD"}, table?.ColumnNames);
        Assert.Contains("Cats", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Food", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
    [Fact]
    public async Task StatisticForADayAllowsToChooseBetweenTodayYesterdayAndEnterCustomDate()
    {
        await _expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        var response = await _botEngine.Proceed("Day expenses (by categories)");

        // Assert
        Assert.NotNull(response.Options);
        
        var buttons = response.Options.AllOptions().Select(_ => _.Text);
        Assert.Contains("Today", buttons);
        Assert.Contains("Yesterday", buttons);
        Assert.Contains("Another day", buttons);
    }
    
    [Fact]
    public async Task StatisticForACustomDay()
    {
        await _expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Day expenses (by categories)");
        await _botEngine.Proceed("Another day");
        await _botEngine.Proceed("22 July 2023");
        var lastMessage = await _botEngine.Proceed("All");
        
        // Assert
        var table = lastMessage.Table;
        
        Assert.NotNull(table);
        Assert.Contains("22 July 2023", table.Subtitle);
        Assert.Contains("Category", table.ColumnNames);
        Assert.Contains("Cats", table.Rows.Select(c => c.FirstColumnValue));
        Assert.Equal(10_000m, table.Rows.First(r => r.FirstColumnValue == "Cats").CurrencyValues[Currency.Amd].Amount);
        Assert.Contains("Total", table.Rows.Select(c => c.FirstColumnValue));
    }
}