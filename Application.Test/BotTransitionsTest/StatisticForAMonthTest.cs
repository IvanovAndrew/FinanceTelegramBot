using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Microsoft.Extensions.DependencyInjection;
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
        var provider = TestServiceFactory.Create(out _expenseRepository, out _dateTimeService, out _messageService, out _, out _);
        _dateTimeService.SetToday(new DateOnly(2023, 7, 24));

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task StatisticForAMonthAllowsToChooseBetweenCurrentPreviousAndEnterCustomMonth()
    {
        // Arrange
        await _expenseRepository.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome()
                {
                    Date = new DateOnly(2023, 5, 22), Category = Categories.Outcome.Pets,
                    Amount = new Money() { Amount = 10_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 6, 23), Category = Categories.Outcome.Pets,
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"),
                    Amount = new Money() { Amount = 1_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"),
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
                },
            }, default);
        await _expenseRepository.SaveIncome(CreateSalary(new DateOnly(2023, 7, 1)), default);

        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        var response = await _botEngine.Proceed("Month expenses (by categories)");

        // Assert
        Assert.NotNull(response.Options);
        
        var buttons = response.Options.Select(_ => _.Text);
        
        Assert.Contains("July 2023", buttons);
        Assert.Contains("June 2023", buttons);
        Assert.Contains("Another month", buttons);
    }

    [Fact]
    public async Task StatisticForACustomMonth()
    {
        await _expenseRepository.SaveAllOutcomes(
            new List<Outcome>()
            {
                new Outcome()
                {
                    Date = new DateOnly(2023, 5, 22), Category = Categories.Outcome.Pets,
                    Amount = new Money() { Amount = 10_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 6, 23), Category = Categories.Outcome.Pets,
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 23), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Snacks"),
                    Amount = new Money() { Amount = 1_000m, Currency = Currency.AMD }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 24), Category = Categories.Outcome.Food, SubCategory = Categories.Outcome.Food.Sub("Products"),
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
                },
            }, default);
        await _expenseRepository.SaveIncome(CreateSalary(new DateOnly(2023, 7, 1)), default);

        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("Statistics");
        await _botEngine.Proceed("Month expenses (by categories)");
        await _botEngine.Proceed("Another month");
        await _botEngine.Proceed("May 2023");
        await _botEngine.Proceed("AMD");


        var table = _messageService.SentMessages.Single().Table;

        // Assert
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("May 2023", table.Subtitle);
        Assert.Contains("Category", table.ColumnNames);
        Assert.Contains("Домашние животные", table.Rows.Select(c => c.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(c => c.FirstColumnValue));
    }

    private Income CreateSalary(DateOnly salaryDay)
    {
        return new Income()
        {
            Category = Categories.Income.Salary,
            Amount = new Money() { Amount = 1_000m, Currency = Currency.AMD },
            Date = salaryDay
        };
    }
}