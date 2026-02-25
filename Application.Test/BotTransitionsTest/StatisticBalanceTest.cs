using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class StatisticBalanceTest 
{
    private readonly BotEngineWrapper _botEngine;
    private readonly DateTimeServiceStub _dateTimeService;
    private readonly MessageServiceMock _messageService;
    private readonly FinanceRepositoryStub _expenseRepository;
    private readonly SalaryDayServiceStub _salaryDayService;

    public StatisticBalanceTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _dateTimeService, out _messageService, out _, out _salaryDayService);
        _dateTimeService.SetToday(new DateOnly(2023, 7, 24));

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task Balance_Since_Previous_Month()
    {
        await _expenseRepository.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        await _expenseRepository.SaveIncome(CreateSalary(), default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Balance");
        await _botEngine.Proceed("June 2023");
        var lastMessage = await _botEngine.Proceed("AMD");

        // Assert
        var table = _messageService.SentMessages.First(t => t.Table != null)?.Table;
        
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
        _salaryDayService.SalaryDay = new DateOnly(2023, 8, 1);
        await _expenseRepository.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        await _expenseRepository.SaveIncome(CreateSalary(), default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Balance");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("January 2023");
        await _botEngine.Proceed("AMD");

        // Assert
        var table = _messageService.SentMessages.First(t => t.Table != null)?.Table;
        
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
        await _expenseRepository.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Pets, Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        await _expenseRepository.SaveIncome(CreateSalary(), default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Balance");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("January 2023");
        await _botEngine.Proceed("AMD");

        // Assert
        Assert.Equal(3, _messageService.SentMessages.Count);

        var (firstMessage, secondMessage) = (_messageService.SentMessages[0], _messageService.SentMessages[1]);

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