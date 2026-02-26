using Domain;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class StateTest
{
    [Fact]
    public async Task ThereAreTwoOptionsInGreetingState()
    {
        // Act
        var scenario = await BotScenario.Start();
        
        // Assert
        Assert.NotEmpty(scenario.LastMessage.Options);
        Assert.Equivalent(new []{"Outcome", "Income", "Statistics"}, scenario.LastMessage.Options.Select(c => c.Text));
    }

    [Theory]
    [InlineData("Outcome")]
    [InlineData("Income")]
    [InlineData("Statistics")]
    public async Task AfterPressingOnAnyButtonInGreetingState_TheGreetingMessageIsDisappeared(string pressedButton)
    {
        // Act
        var scenario = await BotScenario.Start();
        var greetingMessageText = scenario.LastMessage.Text;

        await scenario.ChooseOutcome();

        // Assert
        Assert.DoesNotContain(greetingMessageText, scenario.MessageService.SentMessages.Select(c => c.Text));
    }
    
    [Fact(Skip = "Temporarily ignored")]
    public async Task WhenBackCommandIsExecutedThenLastBotMessageWillBeRemoved()
    {
        // Act
        var scenario = await BotScenario.Start();
        await scenario.ChooseOutcome();
        await scenario.EnterManually();
        await scenario.WithDateToday();
        await scenario.Back();

        // Assert
        Assert.DoesNotContain("Enter the category", scenario.MessageService.SentMessages.Select(c => c.Text));
    }
    
    [Fact(Skip = "Temporarily ignored")]
    public async Task ClickOnCancelButtonCancelsLongTermOperation()
    {
        // Act
        var scenario = await BotScenario.Start();
        scenario.Repo.DelayTime = TimeSpan.FromSeconds(10);
        await scenario.ChooseOutcome();
        await scenario.EnterManually();
        await scenario.GoToPriceInput("today", "Коты", "Корм", "royal canin");
        await scenario.WithPrice("20000 amd");
        
        // Act
        var savingTask = scenario.ConfirmSaving();
        Thread.Sleep(TimeSpan.FromSeconds(1));
        var cancellingTask = scenario.Cancel();

        await Task.WhenAll(savingTask, cancellingTask);

        var savedExpenses = await scenario.Repo.ReadOutcomes(new FinanceFilter(), default);
        
        // Assert
        Assert.Empty(savedExpenses);
    }
    
    [Fact]
    public async Task ThereAreFiveOptionsInStatisticsState()
    {
        // Act
        var scenario = await BotScenario.Start();
        await scenario.SelectStatistics();

        // Assert
        Assert.NotEmpty(scenario.LastMessage.Options);
        Assert.Equivalent(new []{"Balance", "Day expenses (by categories)", "Month expenses (by categories)", "Category expenses (by months)", "Subcategory expenses (overall)", "Subcategory expenses (by months)"}, scenario.LastMessage.Options.Select(c => c.Text));
    }
}