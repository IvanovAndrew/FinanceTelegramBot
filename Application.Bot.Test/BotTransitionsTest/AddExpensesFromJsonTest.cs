using System.Net.Mime;
using Application.Core;
using Application.Test.Stubs;
using Domain;
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
        scenario.MessageService.SavedFiles["1"] = new FileStub(){Text = "{\"messageFiscalSign\": 10, \"user\": \"user\", \"retailPlace\": \"address\", \"dateTime\": \"2023-06-29T20:00:00\", \"items\":[{\"sum\": 100000,\"name\":\"Молоко\"}, {\"sum\": 78000, \"name\":\"Макароны\"}]}"};
        
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
        scenario.MessageService.SavedFiles["1"] = new FileStub(){Text = "test"};
        scenario.ExpenseJsonParser.JsonToCheck.Add("test", 
            new Check()
            {
                Outcomes = new List<Outcome>()
                {
                    new Outcome()
                    {
                        Amount = new Money(){Amount = 10_000, Currency = Currency.RUR},
                        Category = Categories.Outcome.Food,
                        SubCategory = Categories.Outcome.Food.GetSubcategoryByName("products"),
                        Date = new DateOnly(2023, 6, 29),
                        Shop = Shop.Create("Лента"),
                    }
                }
            });
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.ChooseJsonCheck();
        await scenario.LoadFile(telegramFile);
        
        // Assert
        Assert.Contains("All expenses", scenario.LastMessage.Text);
        Assert.Contains("test.json", scenario.LastMessage.Text);
        Assert.Contains("are saved", scenario.LastMessage.Text);
    }

    [Fact]
    public async Task File_Can_Be_Sent_Immediately_After_Start_Command()
    {
        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = MediaTypeNames.Application.Json };
        
        var scenario = await BotScenario.Start();
        scenario.MessageService.SavedFiles["1"] = new FileStub(){Text = "test"};
        scenario.ExpenseJsonParser.JsonToCheck.Add("test", 
            new Check()
            {
                Outcomes = new List<Outcome>()
                {
                    new Outcome()
                    {
                        Amount = new Money(){Amount = 10_000, Currency = Currency.RUR},
                        Category = Categories.Outcome.Food,
                        SubCategory = Categories.Outcome.Food.GetSubcategoryByName("products"),
                        Date = new DateOnly(2023, 6, 29),
                        Shop = Shop.Create("Лента"),
                    }
                }
            });
        
        // Act
        await scenario.LoadFile(telegramFile);
        
        // Assert
        Assert.Contains("All expenses", scenario.LastMessage.Text);
        Assert.Contains("test.json", scenario.LastMessage.Text);
        Assert.Contains("are saved", scenario.LastMessage.Text);
    }
    
    [Fact]
    public async Task File_Can_Be_Sent_Immediately()
    {
        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = MediaTypeNames.Application.Json };
        
        var scenario = BotScenario.Preset();
        scenario.MessageService.SavedFiles["1"] = new FileStub(){Text = "test"};
        scenario.ExpenseJsonParser.JsonToCheck.Add("test", 
            new Check()
            {
                Outcomes = new List<Outcome>()
                {
                    new Outcome()
                    {
                        Amount = new Money(){Amount = 10_000, Currency = Currency.RUR},
                        Category = Categories.Outcome.Food,
                        SubCategory = Categories.Outcome.Food.GetSubcategoryByName("products"),
                        Date = new DateOnly(2023, 6, 29),
                        Shop = Shop.Create("Лента"),
                    }
                }
            });
        
        // Act
        await scenario.LoadFile(telegramFile);
        
        // Assert
        Assert.Contains("All expenses", scenario.LastMessage.Text);
        Assert.Contains("test.json", scenario.LastMessage.Text);
        Assert.Contains("are saved", scenario.LastMessage.Text);
    }
    
    [Fact]
    public void Several_Files_Can_Be_Sent_Immediately()
    {
        var firstFile = new FileInfoStub() { FileId = "1", FileName = "first test.json", MimeType = MediaTypeNames.Application.Json };
        var secondFile = new FileInfoStub() { FileId = "2", FileName = "second test.json", MimeType = MediaTypeNames.Application.Json };
        
        var scenario = BotScenario.Preset();
        scenario.MessageService.SavedFiles["1"] = new FileStub(){Text = "first test"};
        scenario.ExpenseJsonParser.JsonToCheck.Add("first test", 
            new Check()
            {
                Outcomes = new List<Outcome>()
                {
                    new Outcome()
                    {
                        Amount = new Money(){Amount = 10_000, Currency = Currency.RUR},
                        Category = Categories.Outcome.Food,
                        SubCategory = Categories.Outcome.Food.GetSubcategoryByName("products"),
                        Date = new DateOnly(2023, 6, 29),
                        Shop = Shop.Create("Лента"),
                    }
                }
            });
        scenario.MessageService.SavedFiles["2"] = new FileStub(){Text = "second test"};
        scenario.ExpenseJsonParser.JsonToCheck.Add("second test", 
            new Check()
            {
                Outcomes = new List<Outcome>()
                {
                    new Outcome()
                    {
                        Amount = new Money(){Amount = 5_000, Currency = Currency.RUR},
                        Category = Categories.Outcome.ForHouse,
                        Date = new DateOnly(2023, 6, 29),
                        Shop = Shop.Create("Пятерочка"),
                    }
                }
            });
        
        // Act
        var firstTask = scenario.LoadFile(firstFile);
        var secondTask = scenario.LoadFile(secondFile);
        
        Task.WaitAll(firstTask, secondTask);
        
        // Assert
        Assert.Contains(scenario.MessageService.SentMessages, x => x.Text.Contains("first test.json"));
        Assert.Contains(scenario.MessageService.SentMessages, x => x.Text.Contains("second test.json"));
    }
}