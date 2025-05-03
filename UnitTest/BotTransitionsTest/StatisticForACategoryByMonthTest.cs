using Domain;
using Infrastructure;
using NUnit.Framework;
using UnitTest.Extensions;

namespace UnitTest.BotTransitionsTest;

public class StatisticForACategoryByMonthTest
{
    [Test]
    public async Task StatisticForACategoryWithACustomDateRange()
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
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var userSessionService = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSessionService);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Category expenses (by months)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Another month");
        await botEngine.Proceed("January 2022");
        var lastMessage = await botEngine.Proceed("All");
        
        // Assert
        var table = lastMessage.Table;
        Assert.NotNull(table);
        StringAssert.Contains("Statistic", table.Title);
        StringAssert.Contains("Category", table.Subtitle);
        StringAssert.Contains("Food", table.Subtitle);
        CollectionAssert.Contains(table.Rows.Select(r => r.FirstColumnValue), "July 2023");
        CollectionAssert.Contains(table.Rows.Select(r => r.FirstColumnValue), "Total");
    }
    
    [Test]
    public async Task StatisticForACategoryByAPeriod()
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
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        
        var userSession = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSession);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Category expenses (by months)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("January 2023");
        var lastMessage = await botEngine.Proceed("All");

        // Assert
        var table = lastMessage.Table;
        Assert.NotNull(table);
        
        Assert.That(table.Title, Is.EqualTo("Statistic"));
        StringAssert.Contains("Category", table.Subtitle);
        StringAssert.Contains("Food", table.Subtitle);
        CollectionAssert.Contains(table.Rows.Select(r => r.FirstColumnValue), "July 2023");
        CollectionAssert.Contains(table.Rows.Select(r => r.FirstColumnValue), "Total");
    }
}