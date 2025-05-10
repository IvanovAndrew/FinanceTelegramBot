using Domain;
using Infrastructure;
using UnitTest.Extensions;
using Xunit;

namespace UnitTest.BotTransitionsTest;

public class StatisticForASubcategoryTest
{
    [Fact]
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
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("Expenses from July 2022", table.Subtitle);
        Assert.Contains("Category: Food", table.Subtitle);
        Assert.Contains("Subcategory", table.FirstColumnName);
        Assert.Contains("Snacks", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Products", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
    
    [Fact]
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
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("Expenses from March 2022", table.Subtitle);
        Assert.Contains("Category: Food", table.Subtitle);
        Assert.Contains("Snacks", table.Rows.Select(c => c.FirstColumnValue));
        Assert.Contains("Products", table.Rows.Select(c => c.FirstColumnValue));
        Assert.Equal(1_000m, table.Rows.First(c => c.FirstColumnValue == "Snacks").CurrencyValues[Currency.Amd].Amount);
        Assert.Contains("Total", table.Rows.Select(c => c.FirstColumnValue));
    }
}