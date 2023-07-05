using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using StateMachine;

namespace EngineTest;

public class StateTest
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
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        
        // Assert
        CollectionAssert.AreEquivalent(new []{"Outcome", "Statistics"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(b => b.Select(b1 => b1)).Select(c => c.Text));
    }
    
    [TestCase("Outcome")]
    [TestCase("Statistics")]
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
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        var greetingMessage = await botEngine.Proceed("/start");
        var message = await botEngine.Proceed(pressedButton);

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
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        
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
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        var lastMessage = await botEngine.Proceed(answer);
        
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
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("today");
        var lastMessage = await botEngine.Proceed("cats");
        
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
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
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
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("today");
        await botEngine.Proceed("cats");
        await botEngine.Proceed("royal canin");
        var lastMessage = await botEngine.Proceed(price);
        
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
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("today");
        await botEngine.Proceed("cats");
        await botEngine.Proceed("royal canin");
        await botEngine.Proceed("20000 amd");
        var lastMessage = await botEngine.Proceed("Save");

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
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        var lastMessage = await botEngine.Proceed("statistics");

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

    private BotEngineWrapper CreateBotEngineWrapper(Category[] categories, IExpenseRepository expenseRepository, IDateTimeService dateTimeService, TelegramBotMock telegramBot)
    {
        var botEngine = CreateBotEngine(categories, expenseRepository, dateTimeService);
        return new BotEngineWrapper(botEngine, telegramBot);
    }
}