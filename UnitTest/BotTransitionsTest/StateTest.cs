using Domain;
using Infrastructure;
using NUnit.Framework;
using UnitTest.Extensions;

namespace UnitTest.BotTransitionsTest;

public class StateTest
{
    [Test]
    public async Task ThereAreTwoOptionsInGreetingState()
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = [new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" }]
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
        
        // Assert
        CollectionAssert.AreEquivalent(new []{"Outcome", "Income", "Statistics"}, lastMessage.Options.AllOptions().Select(c => c.Text));
    }
    
    [TestCase("Outcome")]
    [TestCase("Income")]
    [TestCase("Statistics")]
    public async Task AfterPressingOnAnyButtonInGreetingState_TheGreetingMessageIsDisappeared(string pressedButton)
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
        var greetingMessage = await botEngine.Proceed("/start");
        var greetingMessageText = greetingMessage.Text;
        await botEngine.Proceed(pressedButton);

        // Assert
        CollectionAssert.DoesNotContain(telegramBot.SentMessages.Select(c => c.Text), greetingMessageText);
    }
    
    [Ignore("Temporarily ignored")]
    [Test]
    public async Task WhenBackCommandIsExecutedThenLastBotMessageWillBeRemoved()
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
        var lastMessage = await botEngine.Proceed("/back");

        // Assert
        Assert.That(telegramBot.SentMessages.Select(c => c.Text), Is.Not.Contains("Enter the category"));
    }
    
    [Test]
    public async Task ClickOnCancelButtonCancelsLongTermOperation()
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
        
        var expenseRepository = new FinanceRepositoryStub() {DelayTime = TimeSpan.FromSeconds(15)};
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
        
        // Act
        var savingTask = botEngine.Proceed("Save");
        Thread.Sleep(TimeSpan.FromSeconds(1));
        var cancellingTask = botEngine.Proceed("/cancel");

        await Task.WhenAll(savingTask, cancellingTask);

        var savedExpenses = await expenseRepository.ReadOutcomes(new FinanceFilter(), default);
        
        // Assert
        Assert.That(savedExpenses.Count, Is.EqualTo(0));
    }
    
    [Test]
    public async Task ThereAreFiveOptionsInStatisticsState()
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
        var lastMessage = await botEngine.Proceed("statistics");

        // Assert
        CollectionAssert.AreEquivalent(new []{"Balance", "Day expenses (by categories)", "Month expenses (by categories)", "Category expenses (by months)", "Subcategory expenses (overall)", "Subcategory expenses (by months)"}, lastMessage.Options.AllOptions().Select(c => c.Text));
    }
}