using Domain;
using Microsoft.Extensions.DependencyInjection;
using UnitTest;
using UnitTest.Extensions;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class AddIncomeManuallyTest
{
    private readonly BotEngineWrapper _botEngine;
    private readonly FinanceRepositoryStub _expenseRepository;

    public AddIncomeManuallyTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _, out _, out _);

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task ClickOnSaveButtonSavesTheIncome()
    {
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("income");
        await _botEngine.Proceed("Another day");
        await _botEngine.Proceed("08.09.2024");
        await _botEngine.Proceed("Other");
        await _botEngine.Proceed("Improvisation class");
        await _botEngine.Proceed("8000 amd");
        var lastMessage = await _botEngine.Proceed("Save");

        var savedIncomes = await _expenseRepository.ReadIncomes(new FinanceFilter(), default);
        var savedIncome = savedIncomes.First();
        
        // Assert
        Assert.EndsWith("Saved", lastMessage.Text);
        Assert.Equal(new DateOnly(2024, 9, 8), savedIncome.Date);
        Assert.Equal(Category.FromString("Other"), savedIncome.Category);
        Assert.Equal("Improvisation class", savedIncome.Description);
        Assert.Equal(new Money(){Amount = 8_000, Currency = Currency.Amd}, savedIncome.Amount);
    }
}