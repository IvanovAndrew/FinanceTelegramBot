using Domain;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class AddExpenseManuallyTest
{
    [Fact]
    public async Task ThereAreThreeDaysForOutcome()
    {
        var scenario = await BotScenario.Start();
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.EnterManually();

        var lastMessage = scenario.LastMessage;
    
        // Assert
        Assert.NotNull(lastMessage.Options);
        Assert.Contains("Today", lastMessage.Options.Select(b => b.Text).ToList()); 
        Assert.Contains("Yesterday", lastMessage.Options.Select(b => b.Text)); 
        Assert.Contains("Another day", lastMessage.Options.Select(b => b.Text)); 
    }
    
    [Theory]
    [InlineData("today")]
    [InlineData("yesterday")]
    public async Task AfterEnteringDateWeChooseACategory(string date)
    {
        var scenario = await BotScenario.Start();
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.EnterManually();
        await scenario.WithDate(date);
        
        // Assert
        var lastMessage = scenario.LastMessage;
        Assert.Equal("Enter the category", lastMessage.Text);
        Assert.NotNull(lastMessage.Options);
        Assert.Equivalent(Categories.Outcome.All.Select(c => c.ShortName?? c.Name), lastMessage.Options.Select(b => b.Text));
    }
    
    [Fact]
    public async Task WhenACategoryWithoutSubcategoryIsChosenTheDescriptionWillBeAsked()
    {
        var scenario = await BotScenario.Start();
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.EnterManually();
        await scenario.WithDateToday();
        await scenario.WithCategory("Онлайн-сервисы");
        
        // Assert
        Assert.Equal("Enter the description", scenario.LastMessage.Text);
    }
    
    [Fact]
    public async Task WhenDescriptionIsAddedThePriceWillBeAsked()
    {
        var scenario = await BotScenario.Start();
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.EnterManually();
        await scenario.GoToPriceInput("today", "Коты", "Корм", "royal canin");
        
        // Assert
        Assert.Equal("Enter the price", scenario.LastMessage.Text);
    }

    [Theory]
    [InlineData("1 рубль")]
    [InlineData("1,1 рубля")]
    [InlineData("1.1 рубля")]
    [InlineData("10 рублей")]
    [InlineData("100 rur")]
    [InlineData("50 amd")]
    [InlineData("50 драм")]
    [InlineData("50 драмов")]
    public async Task WhenThePriceIsAddedThenSaveWillBeAsked(string price)
    {
        var scenario = await BotScenario.Start();
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.EnterManually();
        await scenario.GoToPriceInput("today", "Коты", "Корм", "royal canin");
        await scenario.WithPrice(price);
        
        // Assert
        var lastMessage = scenario.LastMessage;
        Assert.EndsWith("save it?", lastMessage.Text);
        Assert.Equivalent(new []{"Save", "Cancel"}, lastMessage.Options.Select(b => b.Text));
    }
    
    [Fact]
    public async Task ClickOnSaveButtonSavesTheExpense()
    {
        var scenario = await BotScenario.Start();
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.EnterManually();
        await scenario.GoToPriceInput("today", "Коты", "Корм", "royal canin");
        await scenario.WithPrice("20000 amd");
        await scenario.ConfirmSaving();

        var savedExpenses = await scenario.Repo.ReadOutcomes(new FinanceFilter(), default);
        var savedExpense = savedExpenses.First();
        
        // Assert
        Assert.Equal(new DateOnly(2023, 6, 29), savedExpense.Date);
        Assert.Equal(Categories.Outcome.Pets, savedExpense.Category);
        Assert.Equal("food", savedExpense.SubCategory.Code);
        Assert.Equal("royal canin", savedExpense.Description);
        Assert.Equal(new Money(){Amount = 20_000, Currency = Currency.AMD}, savedExpense.Amount);
    }
    
    [Fact]
    public async Task IfWrongPriceIsEnteredThereWillBeANotification()
    {
        var scenario = await BotScenario.Start();
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.EnterManually();
        await scenario.GoToPriceInput("today", "Коты", "Корм", "royal canin");
        await scenario.WithPrice("1999");

        // Assert
        Assert.Contains(scenario.LastMessage.Text, "Missing currency");
    }
    
    [Fact]
    public async Task IfWrongPriceIsEnteredItWillBePossibleToReenterIt()
    {
        var scenario = await BotScenario.Start();
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.EnterManually();
        await scenario.GoToPriceInput("today", "Коты", "Корм", "royal canin");
        await scenario.WithPrice("20000 dam");
        await scenario.WithPrice("1999");
        await scenario.WithPrice("10000 amd");
        await scenario.ConfirmSaving();

        var savedExpenses = await scenario.Repo.ReadOutcomes(new FinanceFilter(), default);
        var savedExpense = savedExpenses.First();
        
        // Assert
        Assert.Equal(new DateOnly(2023, 6, 29), savedExpense.Date);
        Assert.Equal(Categories.Outcome.Pets, savedExpense.Category);
        Assert.Equal("food", savedExpense.SubCategory.Code);
        Assert.Equal("royal canin", savedExpense.Description);
        Assert.Equal(new Money(){Amount = 10_000, Currency = Currency.AMD}, savedExpense.Amount);
    }
}