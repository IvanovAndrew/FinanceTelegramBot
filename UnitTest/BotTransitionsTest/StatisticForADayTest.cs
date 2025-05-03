using Domain;
using Infrastructure;
using NUnit.Framework;
using UnitTest.Extensions;

namespace UnitTest.BotTransitionsTest;

public class StatisticForADayTest
{
    [Test]
    public async Task StatisticForADay()
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new CategoryBuilder("Food").WithSubcategory("Snacks").WithSubcategory("Products").Build(),
            new CategoryBuilder("Cats").Build()
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var userSessionService = new UserSessionService();
        
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSessionService);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Day expenses (by categories)");
        await botEngine.Proceed("Yesterday");
        var lastMessage = await botEngine.Proceed("All");

        // Assert
        var table = lastMessage.Table;
        
        StringAssert.Contains("Statistic", table.Title);
        StringAssert.Contains("23 July 2023", table?.Subtitle ?? string.Empty);
        CollectionAssert.AreEquivalent(new []{"Category", "AMD"}, table?.ColumnNames);
        CollectionAssert.Contains( table.Rows.Select(r => r.FirstColumnValue), "Cats");
        CollectionAssert.Contains( table.Rows.Select(r => r.FirstColumnValue), "Food");
        CollectionAssert.Contains( table.Rows.Select(r => r.FirstColumnValue), "Total");
    }
    
    [Test]
    public async Task StatisticForADayAllowsToChooseBetweenTodayYesterdayAndEnterCustomDate()
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
        var userSessionService = new UserSessionService();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSessionService);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        var response = await botEngine.Proceed("Day expenses (by categories)");
        

        // Assert
        var buttons = response.Options.AllOptions().Select(_ => _.Text);
        CollectionAssert.Contains(buttons, "Today");
        CollectionAssert.Contains(buttons, "Yesterday");
        CollectionAssert.Contains(buttons, "Another day");
    }
    
    [Test]
    public async Task StatisticForACustomDay()
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
        var userSessionService = new UserSessionService();
        
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSessionService);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Day expenses (by categories)");
        await botEngine.Proceed("Another day");
        await botEngine.Proceed("22 July 2023");
        var lastMessage = await botEngine.Proceed("All");
        
        // Assert
        var table = lastMessage.Table;
        
        StringAssert.Contains("22 July 2023", table.Subtitle);
        CollectionAssert.Contains(table.ColumnNames, "Category");
        CollectionAssert.Contains(table.Rows.Select(c => c.FirstColumnValue), "Cats");
        Assert.That(table.Rows.First(r => r.FirstColumnValue == "Cats").CurrencyValues[Currency.Amd].Amount, Is.EqualTo(10_000m));
        CollectionAssert.Contains(table.Rows.Select(c => c.FirstColumnValue), "Total");
    }
}