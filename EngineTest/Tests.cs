using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using StateMachine;

namespace EngineTest;

public class Tests
{
    [Test]
    public async Task ThereAreTwoOptionsInGreetingState()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngine(categories, expenseRepository, dateTimeService);
        
        // Act
        var lastMessage = await botEngine.Proceed(new MessageStub() {Text = "/start"}, telegramBot) as MessageStub;
        
        // Assert
        CollectionAssert.AreEquivalent(new []{"Outcome", "Statistics"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(b => b.Select(b1 => b1)).Select(c => c.Text));
    }
    
    [TestCase("showExpenses")]
    [TestCase("startExpense")]
    public async Task AfterPressingOnAnyButtonInGreetingState_TheGreetingMessageIsDeleted(string pressedButton)
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngine(categories, expenseRepository, dateTimeService);
        
        // Act
        var greetingMessage = await botEngine.Proceed(new MessageStub() {Text = "/start"}, telegramBot) as MessageStub;
        await botEngine.Proceed(new MessageStub() { Text = pressedButton }, telegramBot);

        // Assert
        CollectionAssert.DoesNotContain(telegramBot.SentMessages, greetingMessage);
    }
    
    [Test]
    public async Task ThereAreThreeDaysForOutcome()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngine(categories, expenseRepository, dateTimeService);
        
        // Act
        var lastMessage = await botEngine.Proceed(new MessageStub() {Text = "/start"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "startExpense"}, telegramBot) as MessageStub;
        
        // Assert
        CollectionAssert.AreEquivalent(new []{"Today", "Yesterday", "Other"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(b => b.Select(b1 => b1)).Select(c => c.Text));
    }
    
    [TestCase("today")]
    [TestCase("yesterday")]
    public async Task AfterEnteringDateWeChooseACategory(string answer)
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngine(categories, expenseRepository, dateTimeService);

        // Act
        var lastMessage = await botEngine.Proceed(new MessageStub() {Text = "/start"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "startExpense"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = answer}, telegramBot) as MessageStub;
        
        // Assert
        Assert.That(lastMessage.Text, Is.EqualTo("Enter the category"));
        CollectionAssert.AreEquivalent(new []{"Food", "Cats"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(c => c.Select(b => b.Text)));
    }

    [Test]
    public async Task WhenACategoryWithoutSubcategoryIsChosenTheDescriptionWillBeAsked()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngine(categories, expenseRepository, dateTimeService);
        
        // Act
        var lastMessage = await botEngine.Proceed(new MessageStub() {Text = "/start"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "startExpense"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "today"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "cats"}, telegramBot) as MessageStub;
        
        // Assert
        Assert.That(lastMessage.Text, Is.EqualTo("Write a description"));
    }
    
    [Test]
    public async Task WhenDescriptionIsAddedThePriceWillBeAsked()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngine(categories, expenseRepository, dateTimeService);
        
        // Act
        var lastMessage = await botEngine.Proceed(new MessageStub() {Text = "/start"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "startExpense"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "today"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "cats"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "royal canin"}, telegramBot) as MessageStub;
        
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
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngine(categories, expenseRepository, dateTimeService);
        
        // Act
        var lastMessage = await botEngine.Proceed(new MessageStub() {Text = "/start"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "startExpense"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "today"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "cats"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "royal canin"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = price}, telegramBot) as MessageStub;
        
        // Assert
        StringAssert.EndsWith("save it?", lastMessage.Text);
        CollectionAssert.AreEquivalent(new []{"Save", "Cancel"}, lastMessage.TelegramKeyboard?.Buttons?.SelectMany(row => row.Select(b => b.Text)));
    }
    
    
    [Test]
    public async Task ClickOnSaveButtonSavesTheExpense()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngine(categories, expenseRepository, dateTimeService);
        
        // Act
        var lastMessage = await botEngine.Proceed(new MessageStub() {Text = "/start"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "startExpense"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "today"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "cats"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "royal canin"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "20000 amd"}, telegramBot) as MessageStub;
        lastMessage = await botEngine.Proceed(new MessageStub() {Text = "Save"}, telegramBot) as MessageStub;

        var savedExpenses = await expenseRepository.Read(default);
        var savedExpense = savedExpenses.FirstOrDefault();
        
        // Assert
        Assert.That(() => new DateOnly(2023, 6, 29) == savedExpense.Date);
        Assert.That(savedExpense.Category, Is.EqualTo("Cats"));
        Assert.That(savedExpense.SubCategory, Is.EqualTo(null));
        Assert.That(savedExpense.Description, Is.EqualTo("royal canin"));
        Assert.That(savedExpense.Amount, Is.EqualTo(new Money(){Amount = 20_000, Currency = Currency.Amd}));
    }
    
    [Test]
    public async Task ThereAreThreeOptionsInStatisticsState()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngine(categories, expenseRepository, dateTimeService);
        
        // Act
        await botEngine.Proceed(new MessageStub(){Text = "/start"}, telegramBot);
        var lastMessage = await botEngine.Proceed(new MessageStub(){Text = "showExpenses"}, telegramBot) as MessageStub;

        // Assert
        CollectionAssert.AreEquivalent(new []{"For a day", "For a month", "For a category"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(b => b.Select(b1 => b1)).Select(c => c.Text));
    }

    private StateFactory CreateStateFactory(Category[] categories, IExpenseRepository expenseRepository, IDateTimeService dateTimeService, ILogger<StateFactory> logger)
    {
        return new StateFactory(dateTimeService, new MoneyParser(new CurrencyParser()), categories, expenseRepository, logger);
    }

    private BotEngine CreateBotEngine(Category[] categories, IExpenseRepository expenseRepository, IDateTimeService dateTimeService)
    {
        var logger = new LoggerStub<StateFactory>();
        var stateFactory = CreateStateFactory(categories, expenseRepository, dateTimeService, logger);

        return new BotEngine(stateFactory, logger);
    }
}