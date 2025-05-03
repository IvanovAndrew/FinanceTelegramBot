using Domain;
using Infrastructure;
using NUnit.Framework;
using UnitTest.Extensions;

namespace UnitTest.BotTransitionsTest;


public class AddExpenseManuallyTest
{
    [Test]
    public async Task ThereAreThreeDaysForOutcome()
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
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
        var userSession = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSession);

    
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("By myself");
    
        // Assert
        CollectionAssert.AreEquivalent(new []{"Today", "Yesterday", "Another day"}, lastMessage.Options.AllOptions().Select(b => b.Text)); 
    }
    
    [TestCase("today")]
    [TestCase("yesterday")]
    public async Task AfterEnteringDateWeChooseACategory(string date)
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var outComeCategories = new Category[]
        {
            new CategoryBuilder("Food").WithSubcategory("Snacks").WithSubcategory("Products").Build(),
            new CategoryBuilder("Cats").Build(),
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var userSession = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(outComeCategories, [], expenseRepository, dateTimeService, telegramBot, userSession);

        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        var lastMessage = await botEngine.Proceed(date);
        
        // Assert
        Assert.That(lastMessage.Text, Is.EqualTo("Enter the category"));
        CollectionAssert.AreEquivalent(new []{"Food", "Cats"}, lastMessage.Options.AllOptions().Select(b => b.Text));
    }
    
    [Test]
    public async Task WhenACategoryWithoutSubcategoryIsChosenTheDescriptionWillBeAsked()
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
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
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSessionService);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        var lastMessage = await botEngine.Proceed("cats");
        
        // Assert
        Assert.That(lastMessage.Text, Is.EqualTo("Enter the description"));
    }
    
    [Test]
    public async Task WhenDescriptionIsAddedThePriceWillBeAsked()
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new CategoryBuilder("Food").WithSubcategory("Snacks").WithSubcategory("Products").Build(),
            new CategoryBuilder("Cats").Build(),
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var userSession = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSession);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        await botEngine.Proceed("cats");
        var lastMessage = await botEngine.Proceed("royal canin");
        
        // Assert
        Assert.That(lastMessage.Text, Is.EqualTo("Enter the price"));
    }
    
    [TestCase("1 рубль")]
    [TestCase("10 рублей")]
    [TestCase("100 rur")]
    [TestCase("50 amd")]
    [TestCase("50 драм")]
    [TestCase("50 драмов")]
    public async Task WhenThePriceIsAddedThenSaveWillBeAsked(string price)
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
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
        var userSession = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSession);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        await botEngine.Proceed("cats");
        await botEngine.Proceed("royal canin");
        var lastMessage = await botEngine.Proceed(price);
        
        // Assert
        StringAssert.EndsWith("save it?", lastMessage.Text);
        CollectionAssert.AreEquivalent(new []{"Save", "Cancel"}, lastMessage.Options.AllOptions().Select(b => b.Text));
    }
    
    [Test]
    public async Task ClickOnSaveButtonSavesTheExpense()
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
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
        var userSession = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSession);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        await botEngine.Proceed("cats");
        await botEngine.Proceed("royal canin");
        await botEngine.Proceed("20000 amd");
        var lastMessage = await botEngine.Proceed("Save");

        var savedExpenses = await expenseRepository.ReadOutcomes(new FinanceFilter(), default);
        var savedExpense = savedExpenses.First();
        
        // Assert
        Assert.That(() => new DateOnly(2023, 6, 29) == savedExpense.Date);
        Assert.That(savedExpense.Category, Is.EqualTo("Cats"));
        Assert.That(savedExpense.SubCategory, Is.EqualTo(null));
        Assert.That(savedExpense.Description, Is.EqualTo("royal canin"));
        Assert.That(savedExpense.Amount, Is.EqualTo(new Money(){Amount = 20_000, Currency = Currency.Amd}));
    }
    
    [Test]
    public async Task IfWrongPriceIsEnteredItWillBePossibleToReenterIt()
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
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
        var userSession = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSession);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        await botEngine.Proceed("cats");
        await botEngine.Proceed("royal canin");
        await botEngine.Proceed("20000 dam");
        await botEngine.Proceed("1999");
        await botEngine.Proceed("10000 amd");
        var lastMessage = await botEngine.Proceed("Save");

        var savedExpenses = await expenseRepository.ReadOutcomes(new FinanceFilter(), default);
        var savedExpense = savedExpenses.First();
        
        // Assert
        Assert.That(() => new DateOnly(2023, 6, 29) == savedExpense.Date);
        Assert.That(savedExpense.Category, Is.EqualTo("Cats"));
        Assert.That(savedExpense.SubCategory, Is.EqualTo(null));
        Assert.That(savedExpense.Description, Is.EqualTo("royal canin"));
        Assert.That(savedExpense.Amount, Is.EqualTo(new Money(){Amount = 10_000, Currency = Currency.Amd}));
    }
}