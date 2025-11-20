using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using UnitTest;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class StatisticBalanceTest 
{
    private readonly BotEngineWrapper _botEngine;
    private readonly MessageServiceMock _messageService;
    private readonly FinanceRepositoryStub _expenseRepository;

    public StatisticBalanceTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _, out _messageService, out _);

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task StatisticForADay()
    {
        await _expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats".AsCategory(), Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats".AsCategory(), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food".AsCategory(), SubCategory = "Snacks".AsSubcategory(), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food".AsCategory(), SubCategory = "Products".AsSubcategory(), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Balance");
        await _botEngine.Proceed("June 2023");
        var lastMessage = await _botEngine.Proceed("AMD");

        // Assert
        var table = lastMessage.Table;
        
        Assert.NotNull(table);
        Assert.Contains("Balance", table.Title);
        Assert.Contains("June 2023", table.Subtitle);
        Assert.Equal(new []{"Balance", "AMD"}, table?.ColumnNames);
        Assert.Contains("Income", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Outcome", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
    [Fact]
    public async Task StatisticFromJanuary()
    {
        await _expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats".AsCategory(), Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats".AsCategory(), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food".AsCategory(), SubCategory = "Snacks".AsSubcategory(), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food".AsCategory(), SubCategory = "Products".AsSubcategory(), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Balance");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("January 2023");
        var lastMessage = await _botEngine.Proceed("AMD");

        // Assert
        var table = lastMessage.Table;
        
        Assert.NotNull(table);
        Assert.Contains("Balance", table.Title);
        Assert.Contains("January 2023", table.Subtitle);
        Assert.Equal(new []{"Balance", "AMD"}, table?.ColumnNames);
        Assert.Contains("Income", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Outcome", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
    [Fact]
    public async Task StatisticFromJanuary_Messages()
    {
        await _expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats".AsCategory(), Amount = new Money(){Amount = 10_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats".AsCategory(), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food".AsCategory(), SubCategory = "Snacks".AsSubcategory(), Amount = new Money(){Amount = 1_000m, Currency = Currency.AMD}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food".AsCategory(), SubCategory = "Products".AsSubcategory(), Amount = new Money(){Amount = 5_000m, Currency = Currency.AMD}},
            }, default);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Balance");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("January 2023");
        var lastMessage = await _botEngine.Proceed("AMD");

        // Assert
        Assert.Equal(2, _messageService.SentMessages.Count);

        var (firstMessage, secondMessage) = (_messageService.SentMessages[0], _messageService.SentMessages[1]);

        Assert.Contains("Enter the month", firstMessage.Text);
        Assert.NotNull(secondMessage.Table);
    }
}