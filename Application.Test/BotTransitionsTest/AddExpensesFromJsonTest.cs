using System.Net.Mime;
using Application.Test.Extensions;
using Application.Test.Stubs;
using Microsoft.Extensions.DependencyInjection;
using UnitTest;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class AddExpensesFromJsonTest
{
    private readonly BotEngineWrapper _botEngine;
    private readonly MessageServiceMock _messageService;
    private readonly FinanceRepositoryStub _expenseRepository;

    public AddExpensesFromJsonTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _, out _messageService, out _);

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task AfterClickingOnOutcomesFromJsonTheFileIsRequired()
    {
        // Act
        var lastMessage = await _botEngine.Proceed("/start");
        lastMessage = await _botEngine.Proceed("outcome");
        lastMessage = await _botEngine.Proceed("From check");
        lastMessage = await _botEngine.Proceed("json");
        
        // Assert
        Assert.Equivalent("Paste a json file", lastMessage.Text);
    }
    
    [Theory]
    [InlineData(MediaTypeNames.Application.Pdf)]
    [InlineData(MediaTypeNames.Application.Xml)]
    [InlineData(MediaTypeNames.Text.Plain)]
    public async Task PastedFileShouldHaveJsonFormat(string mimeType)
    {
        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = mimeType };
        _messageService.SavedFiles["1"] = new FileStub(){Text = "{\"dateTime\": \"2023-06-29T20:00:00\", \"items\":[{\"sum\": 100000,\"name\":\"Молоко\"}, {\"sum\": 78000, \"name\":\"Макароны\"}]}"};
        
        // Act
        var lastMessage = await _botEngine.Proceed("/start");
        lastMessage = await _botEngine.Proceed("outcome");
        lastMessage = await _botEngine.Proceed("From check");
        lastMessage = await _botEngine.Proceed("json");
        lastMessage = await _botEngine.ProceedFile(telegramFile);
        
        // Assert
        Assert.Equivalent("Paste a json file", lastMessage.Text);
    }
    
    [Fact]
    public async Task AllOutcomesFromJsonFileAreSaved()
    {
        // Arrange
        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = MediaTypeNames.Application.Json };
        _messageService.SavedFiles["1"] = new FileStub(){Text = "{\"dateTime\": \"2023-06-29T20:00:00\", \"items\":[{\"sum\": 100000,\"name\":\"Молоко\"}, {\"sum\": 78000, \"name\":\"Макароны\"}]}"};
        
        // Act
        var lastMessage = await _botEngine.Proceed("/start");
        lastMessage = await _botEngine.Proceed("outcome");
        lastMessage = await _botEngine.Proceed("From check");
        lastMessage = await _botEngine.Proceed("json");
        lastMessage = await _botEngine.ProceedFile(telegramFile);
        
        // Assert
        Assert.Contains(_messageService.SentMessages, c => c.Text.Contains("All expenses are saved", StringComparison.InvariantCultureIgnoreCase));
    }
}