using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using UnitTest;
using UnitTest.Stubs;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class StatisticForAMonthTest
{
    private readonly BotEngineWrapper _botEngine;
    private readonly MessageServiceMock _messageService;
    private readonly DateTimeServiceStub _dateTimeService;
    private readonly FinanceRepositoryStub _expenseRepository;

    public StatisticForAMonthTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _dateTimeService, out _messageService, out _);
        _dateTimeService.SetToday(new DateOnly(2023, 7, 24));

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task StatisticForAMonthAllowsToChooseBetweenCurrentPreviousAndEnterCustomMonth()
    {
        // Arrange
        await _expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome()
                {
                    Date = new DateOnly(2023, 5, 22), Category = "Cats".AsCategory(),
                    Amount = new Money() { Amount = 10_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 6, 23), Category = "Cats".AsCategory(),
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 23), Category = "Food".AsCategory(), SubCategory = "Snacks".AsSubcategory(),
                    Amount = new Money() { Amount = 1_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 24), Category = "Food".AsCategory(), SubCategory = "Products".AsSubcategory(),
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
                },
            }, default);

        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        var response = await _botEngine.Proceed("Month expenses (by categories)");

        // Assert
        Assert.NotNull(response.Options);
        
        var buttons = response.Options.AllOptions().Select(_ => _.Text);
        
        Assert.Contains("July 2023", buttons);
        Assert.Contains("June 2023", buttons);
        Assert.Contains("January 2023", buttons);
        Assert.Contains("Another month", buttons);
    }

    [Fact]
    public async Task StatisticForACustomMonth()
    {
        
        
        await _expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome()
                {
                    Date = new DateOnly(2023, 5, 22), Category = "Cats".AsCategory(),
                    Amount = new Money() { Amount = 10_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 6, 23), Category = "Cats".AsCategory(),
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 23), Category = "Food".AsCategory(), SubCategory = "Snacks".AsSubcategory(),
                    Amount = new Money() { Amount = 1_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 24), Category = "Food".AsCategory(), SubCategory = "Products".AsSubcategory(),
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
                },
            }, default);

        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Month expenses (by categories)");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("May 2023");
        var lastMessage = await _botEngine.Proceed("All");


        var table = lastMessage.Table;

        // Assert
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("May 2023", table.Subtitle);
        Assert.Contains("Category", table.ColumnNames);
        Assert.Contains("Cats", table.Rows.Select(c => c.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(c => c.FirstColumnValue));
    }
}