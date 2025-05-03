using Domain;
using Infrastructure;
using NUnit.Framework;
using UnitTest.Extensions;

namespace UnitTest.BotTransitionsTest;

public class StatisticForAMonthTest
{
    [Test]
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
        Assert.That(response.Options, Is.Not.Null);
        
        var buttons = response.Options.AllOptions().Select(_ => _.Text);
        
        CollectionAssert.Contains(buttons, "July 2023");
        CollectionAssert.Contains(buttons, "June 2023");
        CollectionAssert.Contains(buttons, "January 2023");
        CollectionAssert.Contains(buttons, "Another month");
    }

    [Test]
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
        Assert.That(table, Is.Not.Null);
        StringAssert.Contains("Statistic", table.Title);
        StringAssert.Contains("May 2023", table.Subtitle);
        CollectionAssert.Contains(table.ColumnNames, "Category");
        CollectionAssert.Contains(table.Rows.Select(c => c.FirstColumnValue), "Cats");
        CollectionAssert.Contains(table.Rows.Select(c => c.FirstColumnValue), "Total");
    }
}