using Domain;
using Infrastructure;
using UnitTest.Extensions;
using Xunit;

namespace UnitTest.BotTransitionsTest;

public class StatisticForASubcategoryByMonthsTest
{
    [Fact]
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
        
        Assert.NotNull(table);
        Assert.Contains("Statistic", table.Title);
        Assert.Contains("July 2023", table.Subtitle);
        Assert.Contains("Category: Food", table.Subtitle);
        Assert.Contains("Subcategory: Snacks", table.Subtitle);
        Assert.Equivalent(new string[] {"Month", "AMD"}, table.ColumnNames);
        Assert.Equal(1000m, table.Rows.First().CurrencyValues[Currency.Amd].Amount);
        Assert.Contains("Total", table.Rows.Last().FirstColumnValue);
    }
    
    [Fact]
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
        Assert.NotNull(table);
        CollectionAssertExtension.AssertOrder(table.Rows.Select(row => row.FirstColumnValue).ToList(), "May 2023", "June 2023", "July 2023");
    }
    
    [Fact]
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
        
        Assert.NotNull(table);
        Assert.Equal("Statistic", table.Title);
        Assert.Contains("January 2022", table.Subtitle);
        Assert.Contains("Category", table.Subtitle);
        Assert.Contains("Food", table.Subtitle);
        Assert.Contains("Subcategory", table.Subtitle);
        Assert.Contains("Snacks", table.Subtitle);
        
        Assert.Contains("July 2023", table.Rows.Select(r => r.FirstColumnValue));
        Assert.Contains("Total", table.Rows.Select(r => r.FirstColumnValue));
    }
}