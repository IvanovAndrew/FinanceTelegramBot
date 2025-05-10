using Domain;
using Infrastructure;
using UnitTest.Extensions;
using Xunit;

namespace UnitTest.BotTransitionsTest;

public class StatisticForAMonthTest
{
    [Fact]
    public async Task StatisticForAMonthAllowsToChooseBetweenCurrentPreviousAndEnterCustomMonth()
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };

        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome()
                {
                    Date = new DateOnly(2023, 5, 22), Category = "Cats",
                    Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 6, 23), Category = "Cats",
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks",
                    Amount = new Money() { Amount = 1_000m, Currency = Currency.Amd }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products",
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
                },
            }, default);
        var userSessionService = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot,
            userSessionService);

        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        var response = await botEngine.Proceed("Month expenses (by categories)");


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
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };

        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome()
                {
                    Date = new DateOnly(2023, 5, 22), Category = "Cats",
                    Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 6, 23), Category = "Cats",
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks",
                    Amount = new Money() { Amount = 1_000m, Currency = Currency.Amd }
                },
                new Outcome()
                {
                    Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products",
                    Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
                },
            }, default);

        var userSessionService = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot,
            userSessionService);

        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Month expenses (by categories)");
        await botEngine.Proceed("Another month");
        await botEngine.Proceed("May 2023");
        var lastMessage = await botEngine.Proceed("All");


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