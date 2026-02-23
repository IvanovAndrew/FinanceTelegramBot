using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class StatisticForADayTest
{
    private readonly BotEngineWrapper _botEngine;
    private readonly DateTimeServiceStub _dateTimeService;
    private readonly MessageServiceMock _messageService;
    private readonly FinanceRepositoryStub _expenseRepository;

    public StatisticForADayTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _dateTimeService, out _messageService, out _, out _);

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }

    
    [Fact]
    public async Task Statistic_For_Yesterday()
    {
        _dateTimeService.SetToday(new DateOnly(2023, 7, 24)); 
        
        await _expenseRepository.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        await _expenseRepository.SaveIncome(new Income() { Date = new DateOnly(2023, 7, 1), Amount = new Money(){Amount = 1000, Currency = Currency.AMD}}, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Day expenses (by categories)");
        await _botEngine.Proceed("Yesterday");
        var lastMessage = await _botEngine.Proceed("AMD");

        // Assert
        var table = lastMessage.Table;
        
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("23 July 2023", table?.Subtitle ?? string.Empty);
        Assert.Equivalent(new []{"Category", "AMD"}, table?.ColumnNames);
        Assert.Contains("Домашние животные", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Еда", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
    [Fact]
    public async Task StatisticForADayAllowsToChooseBetweenTodayYesterdayAndEnterCustomDate()
    {
        await _expenseRepository.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
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
    public async Task Statistic_For_A_Custom_Day()
    {
        await _expenseRepository.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Day expenses (by categories)");
        await _botEngine.Proceed("Another day");
        await _botEngine.Proceed("22 July 2023");
        var messageToCheck = await _botEngine.Proceed("AMD");

        // Assert
        messageToCheck = _messageService.SentMessages[^1];
        
        Assert.NotNull(messageToCheck.Table);
        Assert.Contains("Statistic", messageToCheck.Table.Title);
        Assert.Contains("22 July 2023", messageToCheck.Table.Subtitle);
        Assert.Contains("Category", messageToCheck.Table.FirstColumnName);
        Assert.Contains("Домашние животные", messageToCheck.Table.Rows.Select(r => r.FirstColumnValue));
        Assert.Equal(new Money(){Amount = 10_000, Currency = Currency.AMD}, messageToCheck.Table.Rows.Select(r => r.CurrencyValues[Currency.AMD]).First());
        Assert.Contains("Total", messageToCheck.Table.Rows.Select(r => r.FirstColumnValue));
    }
}