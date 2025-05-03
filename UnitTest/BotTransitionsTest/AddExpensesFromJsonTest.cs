using System.Net.Mime;
using Domain;
using Infrastructure;
using NUnit.Framework;
using UnitTest.Extensions;

namespace UnitTest.BotTransitionsTest;

public class AddExpensesFromJsonTest
{
    [Test]
    public async Task AfterClickingOnOutcomesFromJsonTheFileIsRequired()
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
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("From check");
        lastMessage = await botEngine.Proceed("json");
        
        // Assert
        CollectionAssert.AreEquivalent("Paste a json file", lastMessage.Text);
    }
    
    [TestCase(MediaTypeNames.Application.Pdf)]
    [TestCase(MediaTypeNames.Application.Xml)]
    [TestCase(MediaTypeNames.Text.Plain)]
    public async Task PastedFileShouldHaveJsonFormat(string mimeType)
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
        var userSessionService = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSessionService);

        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = mimeType };
        telegramBot.SavedFiles["1"] = new FileStub(){Text = "{\"dateTime\": \"2023-06-29T20:00:00\", \"items\":[{\"sum\": 100000,\"name\":\"Молоко\"}, {\"sum\": 78000, \"name\":\"Макароны\"}]}"};
        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("From check");
        lastMessage = await botEngine.Proceed("json");
        lastMessage = await botEngine.ProceedFile(telegramFile);
        
        // Assert
        CollectionAssert.AreEquivalent("Paste a json file", lastMessage.Text);
    }
    
    [Test]
    public async Task AllOutcomesFromJsonFileAreSaved()
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

        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = MediaTypeNames.Application.Json };
        telegramBot.SavedFiles["1"] = new FileStub(){Text = "{\"dateTime\": \"2023-06-29T20:00:00\", \"items\":[{\"sum\": 100000,\"name\":\"Молоко\"}, {\"sum\": 78000, \"name\":\"Макароны\"}]}"};
        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("From check");
        lastMessage = await botEngine.Proceed("json");
        lastMessage = await botEngine.ProceedFile(telegramFile);
        
        // Assert
        Assert.That(telegramBot.SentMessages.Any(c => c.Text.Contains("All expenses are saved", StringComparison.InvariantCultureIgnoreCase)));
    }
}