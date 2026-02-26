using System.Net.Mime;
using UnitTest;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class AddExpensesFromJsonTest
{
    [Fact]
    public async Task AfterClickingOnOutcomesFromJsonTheFileIsRequired()
    {
        var scenario = await BotScenario.Start();
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.ChooseJsonCheck();
        
        // Assert
        Assert.Equivalent("Paste a json file", scenario.LastMessage.Text);
    }
    
    [Theory]
    [InlineData(MediaTypeNames.Application.Pdf)]
    [InlineData(MediaTypeNames.Application.Xml)]
    [InlineData(MediaTypeNames.Text.Plain)]
    public async Task PastedFileShouldHaveJsonFormat(string mimeType)
    {
        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = mimeType };
        
        var scenario = await BotScenario.Start();
        scenario.MessageService.SavedFiles["1"] = new FileStub(){Text = "{\"dateTime\": \"2023-06-29T20:00:00\", \"items\":[{\"sum\": 100000,\"name\":\"Молоко\"}, {\"sum\": 78000, \"name\":\"Макароны\"}]}"};
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.ChooseJsonCheck();
        await scenario.LoadFile(telegramFile);
        
        // Assert
        Assert.Equivalent("Paste a json file", scenario.LastMessage.Text);
    }
    
    [Fact]
    public async Task AllOutcomesFromJsonFileAreSaved()
    {
        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = MediaTypeNames.Application.Json };
        
        var scenario = await BotScenario.Start();
        scenario.MessageService.SavedFiles["1"] = new FileStub(){Text = "{\"dateTime\": \"2023-06-29T20:00:00\", \"items\":[{\"sum\": 100000,\"name\":\"Молоко\"}, {\"sum\": 78000, \"name\":\"Макароны\"}]}"};
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.ChooseJsonCheck();
        await scenario.LoadFile(telegramFile);
        
        // Assert
        Assert.Contains(scenario.MessageService.SentMessages, c => c.Text.Contains("All expenses are saved", StringComparison.InvariantCultureIgnoreCase));
    }
}