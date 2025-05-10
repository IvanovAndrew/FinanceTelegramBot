using Domain;
using Infrastructure;
using UnitTest.Extensions;
using Xunit;

namespace UnitTest.BotTransitionsTest;

public class AddExpenseManuallyTest
{
    private readonly MessageServiceMock _telegramBot;
    private readonly DateTimeServiceStub _dateTimeService;
    private readonly FinanceRepositoryStub _expenseRepository;
    private readonly UserSessionService _userSession;

    public AddExpenseManuallyTest()
    {
        _telegramBot = new MessageServiceMock();
        _dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        _expenseRepository = new FinanceRepositoryStub();
        _userSession = new UserSessionService();
    }
    
    [Fact]
    public async Task ThereAreThreeDaysForOutcome()
    {
        // Arrange
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
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, _dateTimeService, _telegramBot, userSession);

    
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("By myself");
    
        // Assert
        Assert.NotNull(lastMessage.Options);
        Assert.Contains("Today", lastMessage.Options.AllOptions().Select(b => b.Text).ToList()); 
        Assert.Contains("Yesterday", lastMessage.Options.AllOptions().Select(b => b.Text)); 
        Assert.Contains("Another day", lastMessage.Options.AllOptions().Select(b => b.Text)); 
    }
    
    [InlineData("today")]
    [InlineData("yesterday")]
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
        Assert.Equal("Enter the category", lastMessage.Text);
        Assert.NotNull(lastMessage.Options);
        Assert.Equivalent(new []{"Food", "Cats"}, lastMessage.Options.AllOptions().Select(b => b.Text));
    }
    
    [Fact]
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
        Assert.Equal("Enter the description", lastMessage.Text);
    }
    
    [Fact]
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
        Assert.Equal("Enter the price", lastMessage.Text);
    }
    
    [InlineData("1 рубль")]
    [InlineData("10 рублей")]
    [InlineData("100 rur")]
    [InlineData("50 amd")]
    [InlineData("50 драм")]
    [InlineData("50 драмов")]
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
        Assert.EndsWith("save it?", lastMessage.Text);
        Assert.NotNull(lastMessage.Options);
        Assert.Equivalent(new []{"Save", "Cancel"}, lastMessage.Options.AllOptions().Select(b => b.Text));
    }
    
    [Fact]
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
        Assert.Equal(new DateOnly(2023, 6, 29), savedExpense.Date);
        Assert.Equal("Cats", savedExpense.Category);
        Assert.Null(savedExpense.SubCategory);
        Assert.Equal("royal canin", savedExpense.Description);
        Assert.Equal(new Money(){Amount = 20_000, Currency = Currency.Amd}, savedExpense.Amount);
    }
    
    [Fact]
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
        Assert.Equal(new DateOnly(2023, 6, 29), savedExpense.Date);
        Assert.Equal("Cats", savedExpense.Category);
        Assert.Null(savedExpense.SubCategory);
        Assert.Equal("royal canin", savedExpense.Description);
        Assert.Equal(new Money(){Amount = 10_000, Currency = Currency.Amd}, savedExpense.Amount);
    }
}