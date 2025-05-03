using Domain;
using Infrastructure;
using NUnit.Framework;
using UnitTest.Extensions;

namespace UnitTest.BotTransitionsTest;

public class StatisticForASubcategoryTest
{
    [Test]
    public async Task StatisticForASubcategory()
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new[]
        {
            new CategoryBuilder("Food").WithSubcategory("Snacks").WithSubcategory("Products").Build(),
            new CategoryBuilder("Cats").WithSubcategory("Snacks").WithSubcategory("Products").Build(),
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
        await botEngine.Proceed("Subcategory expenses (overall)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("July 2022");
        var lastMessage = await botEngine.Proceed("AMD");

        var table = lastMessage.Table;

        // Assert
        StringAssert.Contains("Statistic", table.Title);
        StringAssert.Contains("Expenses from July 2022", table.Subtitle);
        StringAssert.Contains("Category: Food", table.Subtitle);
        StringAssert.Contains("Subcategory", table.FirstColumnName);
        CollectionAssert.Contains(table.Rows.Select(r => r.FirstColumnValue), "Snacks");
        CollectionAssert.Contains(table.Rows.Select(r => r.FirstColumnValue), "Products");
        CollectionAssert.Contains(table.Rows.Select(r => r.FirstColumnValue), "Total");
    }
    
    [Test]
    public async Task StatisticByASubcategoryWithCustomDateRange()
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
        await botEngine.Proceed("Subcategory expenses (overall)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Another month");
        await botEngine.Proceed("March 2022");
        var lastMessage = await botEngine.Proceed("All");

        var table = lastMessage.Table;
        
        // Assert
        StringAssert.Contains("Statistic", table.Title);
        StringAssert.Contains("Expenses from March 2022", table.Subtitle);
        StringAssert.Contains("Category: Food", table.Subtitle);
        CollectionAssert.IsSubsetOf(new [] {"Snacks", "Products"}, table.Rows.Select(c => c.FirstColumnValue));
        Assert.That(table.Rows.First(c => c.FirstColumnValue == "Snacks").CurrencyValues[Currency.Amd].Amount, Is.EqualTo(1_000m));
        CollectionAssert.Contains(table.Rows.Select(c => c.FirstColumnValue), "Total");
    }
}