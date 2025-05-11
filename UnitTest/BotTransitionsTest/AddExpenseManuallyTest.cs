using Domain;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Extensions;
using Xunit;

namespace UnitTest.BotTransitionsTest;

public class AddExpenseManuallyTest
{
    private readonly BotEngineWrapper _botEngine;
    private readonly FinanceRepositoryStub _expenseRepository;

    public AddExpenseManuallyTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _, out _, out _);

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task ThereAreThreeDaysForOutcome()
    {
        // Act
        var lastMessage = await _botEngine.Proceed("/start");
        lastMessage = await _botEngine.Proceed("outcome");
        lastMessage = await _botEngine.Proceed("By myself");
    
        // Assert
        Assert.NotNull(lastMessage.Options);
        Assert.Contains("Today", lastMessage.Options.AllOptions().Select(b => b.Text).ToList()); 
        Assert.Contains("Yesterday", lastMessage.Options.AllOptions().Select(b => b.Text)); 
        Assert.Contains("Another day", lastMessage.Options.AllOptions().Select(b => b.Text)); 
    }
    
    [Theory]
    [InlineData("today")]
    [InlineData("yesterday")]
    public async Task AfterEnteringDateWeChooseACategory(string date)
    {
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("outcome");
        await _botEngine.Proceed("By myself");
        var lastMessage = await _botEngine.Proceed(date);
        
        // Assert
        Assert.Equal("Enter the category", lastMessage.Text);
        Assert.NotNull(lastMessage.Options);
        Assert.Equivalent(new []{"Food", "Cats"}, lastMessage.Options.AllOptions().Select(b => b.Text));
    }
    
    [Fact]
    public async Task WhenACategoryWithoutSubcategoryIsChosenTheDescriptionWillBeAsked()
    {
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("outcome");
        await _botEngine.Proceed("By myself");
        await _botEngine.Proceed("today");
        var lastMessage = await _botEngine.Proceed("cats");
        
        // Assert
        Assert.Equal("Enter the description", lastMessage.Text);
    }
    
    [Fact]
    public async Task WhenDescriptionIsAddedThePriceWillBeAsked()
    {
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("outcome");
        await _botEngine.Proceed("By myself");
        await _botEngine.Proceed("today");
        await _botEngine.Proceed("cats");
        var lastMessage = await _botEngine.Proceed("royal canin");
        
        // Assert
        Assert.Equal("Enter the price", lastMessage.Text);
    }

    [Theory]
    [InlineData("1 рубль")]
    [InlineData("10 рублей")]
    [InlineData("100 rur")]
    [InlineData("50 amd")]
    [InlineData("50 драм")]
    [InlineData("50 драмов")]
    public async Task WhenThePriceIsAddedThenSaveWillBeAsked(string price)
    {
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("outcome");
        await _botEngine.Proceed("By myself");
        await _botEngine.Proceed("today");
        await _botEngine.Proceed("cats");
        await _botEngine.Proceed("royal canin");
        var lastMessage = await _botEngine.Proceed(price);
        
        // Assert
        Assert.EndsWith("save it?", lastMessage.Text);
        Assert.NotNull(lastMessage.Options);
        Assert.Equivalent(new []{"Save", "Cancel"}, lastMessage.Options.AllOptions().Select(b => b.Text));
    }
    
    [Fact]
    public async Task ClickOnSaveButtonSavesTheExpense()
    {
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("outcome");
        await _botEngine.Proceed("By myself");
        await _botEngine.Proceed("today");
        await _botEngine.Proceed("cats");
        await _botEngine.Proceed("royal canin");
        await _botEngine.Proceed("20000 amd");
        var lastMessage = await _botEngine.Proceed("Save");

        var savedExpenses = await _expenseRepository.ReadOutcomes(new FinanceFilter(), default);
        var savedExpense = savedExpenses.First();
        
        // Assert
        Assert.Equal(new DateOnly(2023, 6, 29), savedExpense.Date);
        Assert.Equal("Cats", savedExpense.Category);
        Assert.Null(savedExpense.SubCategory);
        Assert.Equal("royal canin", savedExpense.Description);
        Assert.Equal(new Money(){Amount = 20_000, Currency = Currency.Amd}, savedExpense.Amount);
    }
    
    [Fact]
    public async Task IfWrongPriceIsEnteredItWillBePossibleToReenterIt()
    {
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("outcome");
        await _botEngine.Proceed("By myself");
        await _botEngine.Proceed("today");
        await _botEngine.Proceed("cats");
        await _botEngine.Proceed("royal canin");
        await _botEngine.Proceed("20000 dam");
        await _botEngine.Proceed("1999");
        await _botEngine.Proceed("10000 amd");
        var lastMessage = await _botEngine.Proceed("Save");

        var savedExpenses = await _expenseRepository.ReadOutcomes(new FinanceFilter(), default);
        var savedExpense = savedExpenses.First();
        
        // Assert
        Assert.Equal(new DateOnly(2023, 6, 29), savedExpense.Date);
        Assert.Equal("Cats", savedExpense.Category);
        Assert.Null(savedExpense.SubCategory);
        Assert.Equal("royal canin", savedExpense.Description);
        Assert.Equal(new Money(){Amount = 10_000, Currency = Currency.Amd}, savedExpense.Amount);
    }
}