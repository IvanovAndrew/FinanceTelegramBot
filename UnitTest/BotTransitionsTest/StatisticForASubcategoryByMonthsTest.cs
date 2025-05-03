using Domain;
using Infrastructure;
using NUnit.Framework;
using UnitTest.Extensions;

namespace UnitTest.BotTransitionsTest;

public class StatisticForASubcategoryByMonthsTest
{
    [Test]
    public async Task StatisticForASubcategoryByMonths()
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
        [
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
            }
        ], default);
        
        var userSessionService = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSessionService);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Subcategory expenses (by months)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Snacks");
        await botEngine.Proceed("Another month");
        await botEngine.Proceed("July 2023");
        var lastMessage = await botEngine.Proceed("All");
        
        // Assert
        var table = lastMessage.Table;
        
        StringAssert.Contains("Statistic", table.Title);
        StringAssert.Contains("July 2023", table.Subtitle);
        StringAssert.Contains("Category: Food", table.Subtitle);
        StringAssert.Contains("Subcategory: Snacks", table.Subtitle);
        CollectionAssert.AreEquivalent(new string[] {"Month", "AMD"}, table.ColumnNames);
        Assert.That(table.Rows.First().CurrencyValues[Currency.Amd].Amount, Is.EqualTo(1000m));
        StringAssert.Contains("Total", table.Rows.Last().FirstColumnValue);
    }
    
    [Test]
    public async Task StatisticForASubCategoryByMonthsIsSortedChronologically()
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
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);

        var userSessionSerive = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSessionSerive);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Subcategory expenses (by months)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Snacks");
        await botEngine.Proceed("January 2023");
        var lastMessage = await botEngine.Proceed("AMD");

        var table = lastMessage.Table;
        
        // Assert
        CollectionAssertExtension.AssertOrder(table.Rows.Select(row => row.FirstColumnValue).ToList(), "May 2023", "June 2023", "July 2023");
    }
    
    [Test]
    public async Task StatisticForASubCategoryWithCustomDateRange()
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
        await botEngine.Proceed("Subcategory expenses (by months)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Snacks");
        await botEngine.Proceed("Another month");
        await botEngine.Proceed("January 2022");
        var lastMessage = await botEngine.Proceed("All");
        

        // Assert
        var table = lastMessage.Table;
        
        Assert.That(table.Title, Is.EqualTo("Statistic"));
        StringAssert.Contains("January 2022", table.Subtitle);
        StringAssert.Contains("Category", table.Subtitle);
        StringAssert.Contains("Food", table.Subtitle);
        StringAssert.Contains("Subcategory", table.Subtitle);
        StringAssert.Contains("Snacks", table.Subtitle);
        
        CollectionAssert.Contains(table.Rows.Select(r => r.FirstColumnValue), "July 2023");
        CollectionAssert.Contains(table.Rows.Select(r => r.FirstColumnValue), "Total");
    }
}