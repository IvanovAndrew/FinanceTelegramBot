using Domain;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Extensions;
using Xunit;

namespace UnitTest.BotTransitionsTest;

public class StateTest
{
    private readonly BotEngineWrapper _botEngine;
    private readonly MessageServiceMock _messageService;
    private readonly FinanceRepositoryStub _expenseRepository;

    public StateTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _, out _messageService, out _);

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task ThereAreTwoOptionsInGreetingState()
    {
        // Act
        var lastMessage = await _botEngine.Proceed("/start");
        
        // Assert
        Assert.NotNull(lastMessage.Options);
        Assert.Equivalent(new []{"Outcome", "Income", "Statistics"}, lastMessage.Options.AllOptions().Select(c => c.Text));
    }

    [Theory]
    [InlineData("Outcome")]
    [InlineData("Income")]
    [InlineData("Statistics")]
    public async Task AfterPressingOnAnyButtonInGreetingState_TheGreetingMessageIsDisappeared(string pressedButton)
    {
        // Act
        var greetingMessage = await _botEngine.Proceed("/start");
        var greetingMessageText = greetingMessage.Text;
        await _botEngine.Proceed(pressedButton);

        // Assert
        Assert.DoesNotContain(greetingMessageText, _messageService.SentMessages.Select(c => c.Text));
    }
    
    [Fact(Skip = "Temporarily ignored")]
    public async Task WhenBackCommandIsExecutedThenLastBotMessageWillBeRemoved()
    {
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("outcome");
        await _botEngine.Proceed("By myself");
        await _botEngine.Proceed("today");
        var lastMessage = await _botEngine.Proceed("/back");

        // Assert
        Assert.DoesNotContain("Enter the category", _messageService.SentMessages.Select(c => c.Text));
    }
    
    [Fact]
    public async Task ClickOnCancelButtonCancelsLongTermOperation()
    {
        _expenseRepository.DelayTime = TimeSpan.FromSeconds(15);
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("outcome");
        await _botEngine.Proceed("By myself");
        await _botEngine.Proceed("today");
        await _botEngine.Proceed("cats");
        await _botEngine.Proceed("royal canin");
        await _botEngine.Proceed("20000 amd");
        
        // Act
        var savingTask = _botEngine.Proceed("Save");
        Thread.Sleep(TimeSpan.FromSeconds(1));
        var cancellingTask = _botEngine.Proceed("/cancel");

        await Task.WhenAll(savingTask, cancellingTask);

        var savedExpenses = await _expenseRepository.ReadOutcomes(new FinanceFilter(), default);
        
        // Assert
        Assert.Empty(savedExpenses);
    }
    
    [Fact]
    public async Task ThereAreFiveOptionsInStatisticsState()
    {
        // Act
        await _botEngine.Proceed("/start");
        var lastMessage = await _botEngine.Proceed("statistics");

        // Assert
        Assert.NotNull(lastMessage.Options);
        Assert.Equivalent(new []{"Balance", "Day expenses (by categories)", "Month expenses (by categories)", "Category expenses (by months)", "Subcategory expenses (overall)", "Subcategory expenses (by months)"}, lastMessage.Options.AllOptions().Select(c => c.Text));
    }
}