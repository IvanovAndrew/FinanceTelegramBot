using Domain;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Extensions;
using UnitTest.Stubs;
using Xunit;

namespace UnitTest.BotTransitionsTest;

public class AddExpensesFromFNSTest
{
    private readonly BotEngineWrapper _botEngine;
    private readonly FinanceRepositoryStub _expenseRepository;
    private readonly FnsServiceStub _fnsService;

    public AddExpensesFromFNSTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _, out _, out _fnsService);

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task AddOutcomeByManualRequisites()
    {
        // Arrange
        _fnsService.Responses
            ["t=20230620T0000&s=1000.64&fn=1234567890123456&i=1234&fp=1234567&n=1"] = new List<Outcome>()
        {
            new Outcome
            {
                Category = "Food", SubCategory = "Snacks", Date = new DateOnly(2023, 06, 20),
                Amount = new Money() { Amount = 500m, Currency = Currency.Rur },
            },
            new Outcome
            {
                Category = "Food", SubCategory = "Products", Date = new DateOnly(2023, 06, 20),
                Amount = new Money() { Amount = 500.64m, Currency = Currency.Rur },
            },
        };
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("outcome");
        await _botEngine.Proceed("From Check");
        await _botEngine.Proceed("By Requisites");
        await _botEngine.Proceed("20.06.2023");
        await _botEngine.Proceed("1000.64");
        await _botEngine.Proceed("1234567890123456");
        await _botEngine.Proceed("1234");
        var lastMessage = await _botEngine.Proceed("1234567");

        // Assert
        Assert.Contains("Category: Food", lastMessage.Text);
        Assert.Contains("saved with", lastMessage.Text);
    }
}